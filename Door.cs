using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Easings;
using UnityEditor;
using UnityEngine;

public class Door : Interactive {
    public enum DoorState { closed, opening, closing, open, ajar }
    public enum DoorParity { twoWay, openIn, openOut }
    public bool autoClose;
    public bool latched;
    public List<DoorLock> doorLocks;
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
    // public Transform[] knobs;
    public Transform[] doorknobs;
    Dictionary<Transform, Coroutine> knobCoroutines = new Dictionary<Transform, Coroutine>();
    float angularSpeed;
    LoHi angleBounds;
    float impulse;
    Vector3[] parentOriginalPositions;
    void Awake() {
        knobCoroutines = new Dictionary<Transform, Coroutine>();
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
            case DoorState.ajar:
                HandleImpulse();
                CheckAutoClose();
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
        if (angle + delta > angleBounds.high) {
            delta = angleBounds.high - angle;
            // impulse = -0.1f * impulse;
        } else if (angle + delta < angleBounds.low) {
            delta = angleBounds.low - angle;
            // impulse = -0.1f * impulse;
        }
        if (delta == 0)
            return;
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

    public override ItemUseResult DoAction(Interactor interactor) {
        lastInteractorTransform = interactor.transform;
        // NOTE: assumes that only the player can use the door
        ActivateDoorknob(interactor.transform.position, withKeySet: GameManager.I.gameData.playerState.physicalKeys);
        return ItemUseResult.Empty() with { waveArm = true };
    }
    public bool IsLocked() => doorLocks.Any(doorLock => doorLock.locked); // doorLock.locked;
    public void ActivateDoorknob(Vector3 position, HashSet<int> withKeySet = null) {
        if (withKeySet != null) {
            foreach (DoorLock doorLock in doorLocks) {
                foreach (int keyId in withKeySet) {
                    doorLock.TryKeyUnlock(DoorLock.LockType.physical, keyId);
                }
            }
        }

        switch (state) {
            case DoorState.ajar:
                DoApplyOpening(position);
                break;
            case DoorState.closing:
            case DoorState.closed:
                if (IsLocked()) {
                    Toolbox.RandomizeOneShot(audioSource, lockedSounds);
                    foreach (Transform doorknob in doorknobs)
                        JiggleKnob(doorknob);
                } else {
                    foreach (Transform doorknob in doorknobs)
                        TurnKnob(doorknob);
                    DoApplyOpening(position);
                }
                break;
            case DoorState.open:
                ChangeState(DoorState.closing);
                targetAngle = 0f;
                angularSpeed = manipulationSpeed;
                break;
        }
    }
    void DoApplyOpening(Vector3 position) {
        if (parity == DoorParity.twoWay) {
            if (orientationPlane.GetSide(position)) {
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


    public void Push(Vector3 hitNormal, Vector3 hitPoint) {
        // TODO: fix!?

        // hitpoint is world coordinates.
        Vector3 d = hitPoint - hinges[0].position;
        d.y = 0;
        Vector3 torque = Vector3.Cross(d, -1f * hitNormal);
        impulse += 10f * torque.y;
    }

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
        ChangeState(DoorState.ajar);
    }

    public void TurnKnob(Transform knob) {
        if (knob == null)
            return;
        if (!knobCoroutines.ContainsKey(knob)) {
            knobCoroutines[knob] = StartCoroutine(DoTurnKnobRoutine(knob));
        }
    }
    public void JiggleKnob(Transform knob) {
        if (knob == null)
            return;
        if (!knobCoroutines.ContainsKey(knob)) {
            knobCoroutines[knob] = StartCoroutine(JiggleKnobRoutine(knob));
        }
    }
    public void PickJiggleKnob(DoorLock doorlock) {
        // if (knobs == null || knobs.Count() == 0)
        //     return;
        if (doorlock.rotationElements.Count() == 0) return;
        foreach (Transform knob in doorlock.rotationElements) {
            if (knob == null) continue;
            if (!knobCoroutines.ContainsKey(knob)) {
                knobCoroutines[knob] = StartCoroutine(PickJiggleKnobRoutine(knob));
            }
        }
    }
    IEnumerator DoTurnKnobRoutine(Transform knob) {
        if (knob == null) {
            yield return null;
        } else {
            float timer = 0f;
            float duration = 0.25f;
            while (timer < duration) {
                float turnAngle = (float)PennerDoubleAnimation.Linear(timer, 0f, 90f, duration);
                Quaternion turnRotation = Quaternion.Euler(0f, 0f, turnAngle);
                knob.localRotation = turnRotation;
                timer += Time.deltaTime;
                yield return null;
            }
            timer = 0f;
            while (timer < duration) {
                float turnAngle = (float)PennerDoubleAnimation.Linear(timer, 90f, -90f, duration);
                Quaternion turnRotation = Quaternion.Euler(0f, 0f, turnAngle);
                knob.localRotation = turnRotation;
                timer += Time.deltaTime;
                yield return null;
            }
            knobCoroutines.Remove(knob);
        }
    }
    IEnumerator JiggleKnobRoutine(Transform knob) {
        if (knob == null) {
            yield return null;
        } else {
            float timer = 0f;
            float duration = 0.25f;
            while (timer < duration) {
                float turnAngle = (float)PennerDoubleAnimation.BounceEaseOut(timer, 15f, -15f, duration);
                Quaternion turnRotation = Quaternion.Euler(0f, 0f, turnAngle);
                knob.localRotation = turnRotation;
                timer += Time.deltaTime;
                yield return null;
            }

            knobCoroutines.Remove(knob);
        }
    }
    IEnumerator PickJiggleKnobRoutine(Transform knob) {
        float timer = 0f;
        float duration = Random.Range(0.05f, 0.15f);
        float startAngle = knob.localRotation.eulerAngles.z;
        if (startAngle > 180) startAngle -= 360f;
        float offset = Random.Range(-10f, 10f);
        float finalAngle = Mathf.Clamp(startAngle + offset, -10f, 10f);
        while (timer < duration) {
            float turnAngle = (float)PennerDoubleAnimation.CircEaseIn(timer, startAngle, finalAngle - startAngle, duration);
            Quaternion turnRotation = Quaternion.Euler(0f, 0f, turnAngle);
            knob.localRotation = turnRotation;
            timer += Time.deltaTime;
            yield return null;
        }
        knobCoroutines.Remove(knob);
    }
}
