using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : Interactive {
    public enum DoorState { closed, opening, closing, open }
    public enum DoorParity { twoWay, openIn, openOut }
    public bool autoClose;
    public DoorParity parity;
    private DoorState _state;
    public DoorState state {
        get { return _state; }
    }
    public Transform hinge;
    private Transform _myTransform;
    private Plane orientationPlane;
    public Transform myTransform {
        get {
            if (_myTransform == null) {
                _myTransform = transform;
            }
            return _myTransform;
        }
    }
    public float angle;
    public float targetAngle;
    public AudioSource audioSource;
    public AudioClip[] openSounds;
    public AudioClip[] closeSounds;
    public float manipulationSpeed = 150f;
    public float autoCloseSpeed = 80f;
    private static readonly float ANGLE_THRESHOLD = 0.1f;
    private Transform lastInteractorTransform;
    float angularSpeed;
    LoHi angleBounds;
    float impulse;

    void Awake() {
        orientationPlane = new Plane(transform.forward, hinge.position);
        audioSource = Toolbox.SetUpAudioSource(gameObject);
        angleBounds = parity switch {
            DoorParity.openIn => new LoHi(-90f, 0f),
            DoorParity.openOut => new LoHi(0f, 90f),
            DoorParity.twoWay => new LoHi(-90f, 90f),
            _ => new LoHi(-90f, 90f)
        };
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
                Toolbox.RandomizeOneShot(audioSource, closeSounds);
                break;
            default:
                break;
        }
    }
    void OnStateExit(DoorState fromState, DoorState toState) {
        switch (fromState) {
            case DoorState.closed:
                Toolbox.RandomizeOneShot(audioSource, openSounds);
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
        myTransform.RotateAround(hinge.position, Vector3.up, delta);
        // TODO: check after rotation for collision
        // TODO: check for closing, opening
    }

    void OnCollisionEnter(Collision col) {
        // Debug.Log($"on collision enter: {col.gameObject}");
    }

    public override void DoAction(Interactor interactor) {
        // throw new System.NotImplementedException();
        lastInteractorTransform = interactor.transform;
        switch (state) {
            case DoorState.closing:
            case DoorState.closed:
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
        // hitpoint is world coordinates.
        // Debug.Log($"pushing: {hitNormal} {hitPoint}");
        Vector3 d = hitPoint - hinge.position;
        d.y = 0;
        // float torque = d.magnitude * hitNormal.magnitude;
        Vector3 torque = Vector3.Cross(d, -1f * hitNormal);
        // Debug.Log($"torque: {torque}");
        // Rotate(f * torque.y);
        impulse += 10f * torque.y;
    }
}
