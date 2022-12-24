using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Easings;
using UnityEngine;
[System.Serializable]
public class Lock {
    public enum LockType { physical, electronic }
    public Door door;
    public LockType lockType;
    public bool locked;
    public int lockId;
    public AudioClip[] unlockSounds;

    public bool TryKey(LockType keyType, int keyId) {
        if (keyType == lockType && keyId == lockId) {
            this.locked = !this.locked;
            Toolbox.RandomizeOneShot(door.audioSource, unlockSounds);
            return true;
        }
        return false;
    }

    public void PickLock() {
        if (lockType == LockType.physical) {
            this.locked = false;
        }
    }
}


public class Door : Interactive {
    public enum DoorState { closed, opening, closing, open }
    public enum DoorParity { twoWay, openIn, openOut }
    public bool autoClose;
    public bool latched;
    // public bool locked;
    public Lock doorLock;
    public DoorParity parity;
    private DoorState _state;
    public DoorState state {
        get { return _state; }
    }
    public Transform[] hinges;
    public Transform[] parents;
    private Transform _myTransform;
    private Plane orientationPlane;
    public Transform[] doorTransforms;
    public float angle;
    public float targetAngle;
    public AudioSource audioSource;
    public AudioClip[] openSounds;
    public AudioClip[] closeSounds;
    public AudioClip[] lockedSounds;
    public AudioClip[] unlockSounds;
    public AudioClip[] unlatchSounds;
    public float manipulationSpeed = 150f;
    public float autoCloseSpeed = 80f;
    private static readonly float ANGLE_THRESHOLD = 0.1f;
    private Transform lastInteractorTransform;
    public Transform[] knobs;
    Coroutine turnKnobCoroutine;
    float angularSpeed;
    LoHi angleBounds;
    float impulse;
    Vector3[] parentOriginalPositions;
    void Awake() {
        // parentOriginalPositions = parent.position;
        parentOriginalPositions = parents.Select(p => p.position).ToArray();
        orientationPlane = new Plane(transform.forward, hinges[0].position);

        audioSource = Toolbox.SetUpAudioSource(gameObject);
        angleBounds = parity switch {
            DoorParity.openIn => new LoHi(-90f, 0f),
            DoorParity.openOut => new LoHi(0f, 90f),
            DoorParity.twoWay => new LoHi(-90f, 90f),
            _ => new LoHi(-90f, 90f)
        };
        latched = true;
        ChangeState(DoorState.closed);
    }
    void Update() {
        switch (state) {
            case DoorState.closing:
                MoveTowardTargetAngle();
                break;
            case DoorState.opening:
                MoveTowardTargetAngle();
                CheckAutoClose();
                break;
            case DoorState.open:
                HandleImpulse();
                CheckAutoClose();
                break;
            case DoorState.closed:
                HandleImpulse();
                break;
        }
    }
    void HandleImpulse() {
        if (impulse == 0)
            return;
        if (latched) {
            impulse = 0;
            return;
        }
        // check closed & latched
        switch (parity) {
            case DoorParity.twoWay:     // (-90, 90)
                if (Mathf.Abs(impulse) < 50f && Mathf.Abs(angle) < ANGLE_THRESHOLD) {
                    ChangeState(DoorState.closed);
                }
                break;
            case DoorParity.openIn:     // (-90, 0)
                if (impulse > 0 && Mathf.Abs(angle) < ANGLE_THRESHOLD && state != DoorState.closed) {
                    ChangeState(DoorState.closed);
                } else if (impulse < 0 && state == DoorState.closed) {
                    ChangeState(DoorState.open);
                }
                break;
            case DoorParity.openOut:    // (0, 90)
                if (impulse < 0 && Mathf.Abs(angle) < ANGLE_THRESHOLD && state != DoorState.closed) {
                    ChangeState(DoorState.closed);
                } else if (impulse > 0 && state == DoorState.closed) {
                    ChangeState(DoorState.open);
                }
                break;
        }
        if (Mathf.Abs(impulse) > ANGLE_THRESHOLD) {
            float delta = Mathf.Max(0.1f * Mathf.Abs(impulse), 10f * Time.deltaTime);
            if (impulse > 0) {
                impulse -= delta;
                impulse = Mathf.Max(0, impulse);
                Rotate(delta);
            } else if (impulse < 0) {
                impulse += delta;
                impulse = Mathf.Min(0, impulse);
                Rotate(-1f * delta);
            }
        }
    }
    void MoveTowardTargetAngle() {
        if (Mathf.Abs(angle - targetAngle) < ANGLE_THRESHOLD) {
            if (state == DoorState.opening) {
                ChangeState(DoorState.open);
            } else if (state == DoorState.closing) {
                ChangeState(DoorState.closed);
            }
        } else if (angle < targetAngle) {
            Rotate(angularSpeed * Time.deltaTime);
        } else if (angle > targetAngle) {
            Rotate(-1f * angularSpeed * Time.deltaTime);
        }
    }
    void CheckAutoClose() {
        if (autoClose && lastInteractorTransform != null) {
            if (Vector3.Distance(lastInteractorTransform.position, transform.position) > 2f) {
                ChangeState(DoorState.closing);
                targetAngle = 0f;
                angularSpeed = autoCloseSpeed;
            }
        }
    }
    void ChangeState(DoorState newState) {
        if (newState == state) return;
        DoorState tempInitialState = state;
        OnStateExit(tempInitialState, newState);
        _state = newState;
        OnStateEnter(tempInitialState, newState);
        // Debug.Log($"{tempInitialState} -> {newState}");
    }

    void OnStateEnter(DoorState fromState, DoorState toState) {
        switch (toState) {
            case DoorState.closed:
                impulse = 0;
                latched = true;
                Toolbox.RandomizeOneShot(audioSource, closeSounds);
                break;
            default:
                break;
        }
    }
    void OnStateExit(DoorState fromState, DoorState toState) {
        switch (fromState) {
            case DoorState.closed:
                if (latched) {
                    Toolbox.RandomizeOneShot(audioSource, openSounds);
                }
                latched = false;
                break;
            default:
                break;
        }
    }

    void Rotate(float delta) {
        // Debug.Log($"{angle} {delta} {angleBounds}");
        if (angle + delta > angleBounds.high) {
            delta = angleBounds.high - angle;
            // impulse = -0.1f * impulse;
        } else if (angle + delta < angleBounds.low) {
            delta = angleBounds.low - angle;
            // impulse = -0.1f * impulse;
        }
        if (delta == 0)
            return;
        // Debug.Log($"{delta}");
        angle += delta;
        float offAxis = -0.05f * Mathf.Sin(angle * (2 * Mathf.PI / 360));
        float parity = 1f;
        for (int i = 0; i < doorTransforms.Length; i++) {
            doorTransforms[i].RotateAround(hinges[i].position, hinges[i].up, parity * delta);
            parents[i].position = parentOriginalPositions[i] + offAxis * hinges[i].right;
            parity *= -1f;
        }

        // TODO: check after rotation for collision
    }

    void OnCollisionEnter(Collision col) {
        // Debug.Log($"on collision enter: {col.gameObject}");
    }

    public override void DoAction(Interactor interactor) {
        // throw new System.NotImplementedException();
        lastInteractorTransform = interactor.transform;
        ActivateDoorknob(interactor.transform);
    }
    public bool IsLocked() => doorLock.locked;
    public void ActivateDoorknob(Transform userTransform) {
        switch (state) {
            case DoorState.closing:
            case DoorState.closed:
                if (IsLocked()) {
                    Toolbox.RandomizeOneShot(audioSource, lockedSounds);
                    JiggleKnob();
                } else {
                    TurnKnob();
                    if (parity == DoorParity.twoWay) {
                        if (orientationPlane.GetSide(interactor.transform.position)) {
                            ChangeState(DoorState.opening);
                            targetAngle = 90f;
                        } else {
                            ChangeState(DoorState.opening);
                            targetAngle = -90f;
                        }
                    } else if (parity == DoorParity.openIn) {
                        ChangeState(DoorState.opening);
                        targetAngle = -90f;
                    } else if (parity == DoorParity.openOut) {
                        ChangeState(DoorState.opening);
                        targetAngle = 90f;
                    }
                    angularSpeed = manipulationSpeed;
                }
                break;
            case DoorState.opening:
            case DoorState.open:
                ChangeState(DoorState.closing);
                targetAngle = 0f;
                angularSpeed = manipulationSpeed;
                break;
        }
    }


    public void Push(Vector3 hitNormal, Vector3 hitPoint) {
        // TODO: fix!?

        // hitpoint is world coordinates.
        Vector3 d = hitPoint - hinges[0].position;
        d.y = 0;
        Vector3 torque = Vector3.Cross(d, -1f * hitNormal);
        impulse += 10f * torque.y;
    }

    // public void Unlock() {
    //     if (locked) {
    //         Toolbox.RandomizeOneShot(audioSource, unlockSounds);
    //         locked = false;
    //     }
    // }
    public void Unlatch() {
        if (latched) {
            Toolbox.RandomizeOneShot(audioSource, unlatchSounds);
            latched = false;
        }
    }
    public void PushOpenSlightly(Transform opener) {
        if (parity == DoorParity.twoWay) {
            if (orientationPlane.GetSide(opener.position)) {
                impulse = 10;
            } else {
                impulse = -10;
            }
        } else if (parity == DoorParity.openIn) {
            impulse = -10f;
        } else if (parity == DoorParity.openOut) {
            impulse = 10f;
        }
    }

    public void TurnKnob() {
        if (turnKnobCoroutine == null) {
            turnKnobCoroutine = StartCoroutine(DoTurnKnobRoutine());
        }
    }
    public void JiggleKnob() {
        if (turnKnobCoroutine == null) {
            turnKnobCoroutine = StartCoroutine(JiggleKnobRoutine());
        }
    }
    public void PickJiggleKnob() {
        if (turnKnobCoroutine == null) {
            turnKnobCoroutine = StartCoroutine(PickJiggleKnobRoutine());
        }
    }
    IEnumerator DoTurnKnobRoutine() {
        float timer = 0f;
        float duration = 0.25f;
        while (timer < duration) {
            float turnAngle = (float)PennerDoubleAnimation.Linear(timer, 0f, 90f, duration);
            Quaternion turnRotation = Quaternion.Euler(0f, 0f, turnAngle);
            foreach (Transform knob in knobs) {
                knob.localRotation = turnRotation;

            }
            timer += Time.deltaTime;
            yield return null;
        }
        timer = 0f;
        while (timer < duration) {
            float turnAngle = (float)PennerDoubleAnimation.Linear(timer, 90f, -90f, duration);
            Quaternion turnRotation = Quaternion.Euler(0f, 0f, turnAngle);
            foreach (Transform knob in knobs) {

                knob.localRotation = turnRotation;
            }
            timer += Time.deltaTime;
            yield return null;
        }
        turnKnobCoroutine = null;
    }
    IEnumerator JiggleKnobRoutine() {
        float timer = 0f;
        float duration = 0.25f;
        while (timer < duration) {
            float turnAngle = (float)PennerDoubleAnimation.BounceEaseOut(timer, 15f, -15f, duration);
            Quaternion turnRotation = Quaternion.Euler(0f, 0f, turnAngle);
            foreach (Transform knob in knobs) {

                knob.localRotation = turnRotation;
            }
            timer += Time.deltaTime;
            yield return null;
        }
        turnKnobCoroutine = null;
    }
    IEnumerator PickJiggleKnobRoutine() {
        float timer = 0f;
        float duration = Random.Range(0.05f, 0.15f);
        float startAngle = knobs[0].localRotation.eulerAngles.z;
        if (startAngle > 180) startAngle -= 360f;
        float offset = Random.Range(-10f, 10f);
        float finalAngle = Mathf.Clamp(startAngle + offset, -10f, 10f);
        while (timer < duration) {
            float turnAngle = (float)PennerDoubleAnimation.CircEaseIn(timer, startAngle, finalAngle - startAngle, duration);
            Quaternion turnRotation = Quaternion.Euler(0f, 0f, turnAngle);
            foreach (Transform knob in knobs) {
                knob.localRotation = turnRotation;
            }
            timer += Time.deltaTime;
            yield return null;
        }
        turnKnobCoroutine = null;
    }
}
