using System.Collections;
using System.Collections.Generic;
using Easings;
using UnityEngine;
public class HeavyDoor : Interactive {
    enum State { none, open, closed }
    State _state;
    public bool preventClose;
    public AudioSource audioSource;
    public AudioClip[] doorOpen;
    public AudioClip[] doorCloseEnd;
    public AudioClip[] doorCloseStart;
    public AudioClip[] doorOpenEnd;
    Coroutine coroutine;
    // public DoorLock doorLock;
    public KeycardReader keycardReader;
    [Header("upper door")]
    public Transform upperDoor;
    public Vector3 upperDoorLocalClosedPosition;
    public Vector3 upperDoorLocalOpenPosition;
    [Header("lower door")]
    public Transform lowerDoor;
    public Vector3 lowerDoorLocalClosedPosition;
    public Vector3 lowerDoorLocalOpenPosition;
    public override ItemUseResult DoAction(Interactor interactor) {
        bool isUnlocked = keycardReader != null ? keycardReader.TryUnlock() : false;

        switch (_state) {
            case State.none:
            case State.closed:
                if (isUnlocked)
                    OpenDoors();
                break;
            case State.open:
                if (preventClose) {
                    if (coroutine != null) {
                        StopCoroutine(coroutine);
                    }
                    coroutine = StartCoroutine(PreventCloseDoorRoutine());
                } else {
                    CloseDoors();

                }
                break;
        }
        return ItemUseResult.Empty() with {
            waveArm = true
        };
    }

    public void OpenDoors(bool silent = false) {
        ChangeState(State.open, silent: silent);
    }
    public void CloseDoors(bool silent = false) {
        ChangeState(State.closed, silent: silent);
    }

    void ChangeState(State newState, bool silent = false) {
        State tempInitialState = _state;
        OnStateExit(tempInitialState, newState);
        _state = newState;
        OnStateEnter(tempInitialState, newState);
    }

    void OnStateExit(State oldState, State newState) {
        if (coroutine != null) {
            StopCoroutine(coroutine);
        }
        if (newState == oldState) return;
    }

    void OnStateEnter(State oldState, State newState) {
        if (newState == oldState) return;
        switch (newState) {
            case State.open:
                coroutine = StartCoroutine(OpenDoorRoutine());
                break;
            case State.closed:
                coroutine = StartCoroutine(CloseDoorRoutine());
                break;
        }
    }

    IEnumerator OpenDoorRoutine() {
        Toolbox.RandomizeOneShot(audioSource, doorOpen);
        Vector3 upperStart = upperDoor.localPosition;
        Vector3 lowerStart = lowerDoor.localPosition;
        yield return Toolbox.Ease(null, 2f, 0, 1f, PennerDoubleAnimation.CubicEaseInOut, (amount) => {
            upperDoor.localPosition = Vector3.Lerp(upperStart, upperDoorLocalOpenPosition, amount);
            lowerDoor.localPosition = Vector3.Lerp(lowerStart, lowerDoorLocalOpenPosition, amount);
        });
        upperDoor.localPosition = upperDoorLocalOpenPosition;
        lowerDoor.localPosition = lowerDoorLocalOpenPosition;
        Toolbox.RandomizeOneShot(audioSource, doorOpenEnd);
    }

    IEnumerator CloseDoorRoutine() {
        Toolbox.RandomizeOneShot(audioSource, doorCloseStart);
        Vector3 upperStart = upperDoor.localPosition;
        Vector3 lowerStart = lowerDoor.localPosition;

        yield return Toolbox.Ease(null, 2f, 0, 1f, PennerDoubleAnimation.CubicEaseInOut, (amount) => {
            upperDoor.localPosition = Vector3.Lerp(upperStart, upperDoorLocalClosedPosition, amount);
            lowerDoor.localPosition = Vector3.Lerp(lowerStart, lowerDoorLocalClosedPosition, amount);
        });

        upperDoor.localPosition = upperDoorLocalClosedPosition;
        lowerDoor.localPosition = lowerDoorLocalClosedPosition;

        yield return new WaitForSecondsRealtime(0.1f);

        Toolbox.RandomizeOneShot(audioSource, doorCloseEnd);
    }

    IEnumerator PreventCloseDoorRoutine() {
        Toolbox.RandomizeOneShot(audioSource, doorCloseStart);
        Vector3 upperStart = Vector3.Lerp(upperDoorLocalOpenPosition, upperDoorLocalClosedPosition, 0.2f);// upperDoor.localPosition;
        Vector3 lowerStart = Vector3.Lerp(lowerDoorLocalOpenPosition, lowerDoorLocalClosedPosition, 0.2f);

        yield return Toolbox.Ease(null, 0.5f, 0, 1f, PennerDoubleAnimation.BounceEaseOut, (amount) => {
            upperDoor.localPosition = Vector3.Lerp(upperStart, upperDoorLocalOpenPosition, amount);
            lowerDoor.localPosition = Vector3.Lerp(lowerStart, lowerDoorLocalOpenPosition, amount);
        });

        upperDoor.localPosition = upperDoorLocalOpenPosition;
        lowerDoor.localPosition = lowerDoorLocalOpenPosition;

        yield return new WaitForSecondsRealtime(0.1f);

        Toolbox.RandomizeOneShot(audioSource, doorCloseEnd);
    }

}
