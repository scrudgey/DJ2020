using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Easings;
using UnityEngine;
public class SlidingDoor : Interactive {
    enum State { none, open, closed }
    State _state;
    public AudioSource audioSource;
    public AudioClip[] doorOpen;
    public AudioClip[] doorCloseEnd;
    public AudioClip[] doorCloseStart;
    public ElevatorDoorData doorData;
    public KeycardReader keycardReader;
    Coroutine coroutine;
    public override ItemUseResult DoAction(Interactor interactor) {
        bool isLocked = keycardReader.doorLock.isActiveAndEnabled && keycardReader.doorLock.locked;

        switch (_state) {
            case State.none:
            case State.closed:
                if (!isLocked)
                    OpenDoors();
                break;
            case State.open:
                CloseDoors();
                break;
        }
        return ItemUseResult.Empty() with {
            waveArm = true,
            showKeyMenu = isLocked,
            doorlocks = new List<DoorLock> { keycardReader.doorLock }
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
        OnElevatorStateEnter(tempInitialState, newState, silent: silent);
    }

    void OnStateExit(State oldState, State newState) {
        if (coroutine != null) {
            StopCoroutine(coroutine);
        }
        if (newState == oldState) return;

    }
    void OnElevatorStateEnter(State oldState, State newState, bool silent = false) {
        if (newState == oldState) return;
        switch (newState) {
            case State.open:
                coroutine = StartCoroutine(OpenDoorRoutine(doorData, silent: silent));
                break;
            case State.closed:
                coroutine = StartCoroutine(CloseDoorRoutine(doorData, silent: silent));
                break;
        }
    }

    IEnumerator OpenDoorRoutine(ElevatorDoorData data, bool silent = false) {
        if (!silent)
            Toolbox.RandomizeOneShot(audioSource, doorOpen);
        yield return Toolbox.Ease(null, 1f, data.closedLocalPosition.x, data.openLocalPosition.x, PennerDoubleAnimation.Linear, (amount) => {
            Vector3 newPos = new Vector3(amount, data.openLocalPosition.y, data.openLocalPosition.z);
            data.door.position = transform.TransformPoint(newPos);
        });
    }

    IEnumerator CloseDoorRoutine(ElevatorDoorData data, bool silent = false) {
        if (!silent)
            Toolbox.RandomizeOneShot(audioSource, doorCloseStart);
        yield return Toolbox.ChainCoroutines(Toolbox.Ease(null, 1f, data.openLocalPosition.x, data.closedLocalPosition.x, PennerDoubleAnimation.QuadEaseOut, (amount) => {
            Vector3 newPos = new Vector3(amount, data.closedLocalPosition.y, data.closedLocalPosition.z);
            data.door.position = transform.TransformPoint(newPos);

        }),
        new WaitForSecondsRealtime(0.1f),
        Toolbox.CoroutineFunc(() => {
            if (!silent)
                Toolbox.RandomizeOneShot(audioSource, doorCloseEnd);
        })
        );
    }

}
