using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Easings;
using UnityEngine;

[System.Serializable]
public class ElevatorDoorData {
    public Transform door;
    public Vector3 closedLocalPosition;
    public Vector3 openLocalPosition;
}

public class ElevatorDoors : MonoBehaviour {
    enum State { none, open, closed }
    State _state;
    public ElevatorDoorData[] doorDatas;
    Dictionary<ElevatorDoorData, Coroutine> coroutines;
    public Transform indicatorTransform;
    public Transform callbuttonTransform;
    public bool doorsAreClosed;
    public ElevatorIndicator elevatorIndicator;
    public ElevatorCallButton callButton;

    public AudioSource audioSource;
    public AudioClip[] doorOpen;
    public AudioClip[] doorCloseEnd;
    public AudioClip[] doorCloseStart;

    void Awake() {
        indicatorTransform.SetParent(null, true);
        callbuttonTransform.SetParent(null, true);
        foreach (ElevatorDoorData data in doorDatas) {
            data.door.SetParent(null, true);
        }
    }

    void Start() {
        InitializeCoroutines();
    }
    void InitializeCoroutines() {
        if (coroutines == null)
            coroutines = new Dictionary<ElevatorDoorData, Coroutine>();
    }
    public void OpenDoors(bool silent = false) {
        ChangeState(State.open, silent: silent);
    }
    public void CloseDoors(bool silent = false) {
        ChangeState(State.closed, silent: silent);
        elevatorIndicator.ShowLight(false);
    }

    void ChangeState(State newState, bool silent = false) {
        InitializeCoroutines();
        State tempInitialState = _state;
        OnStateExit(tempInitialState, newState);
        _state = newState;
        OnElevatorStateEnter(tempInitialState, newState, silent: silent);
    }

    void OnStateExit(State oldState, State newState) {
        foreach (ElevatorDoorData data in doorDatas) {
            if (coroutines != null && coroutines.ContainsKey(data) && coroutines[data] != null) {
                StopCoroutine(coroutines[data]);
            }
        }
        if (newState == oldState) return;

    }
    void OnElevatorStateEnter(State oldState, State newState, bool silent = false) {
        if (newState == oldState) return;
        switch (newState) {
            case State.open:
                foreach (ElevatorDoorData data in doorDatas) {
                    coroutines[data] = StartCoroutine(OpenDoorRoutine(data, silent: silent));
                }
                break;
            case State.closed:
                foreach (ElevatorDoorData data in doorDatas) {
                    coroutines[data] = StartCoroutine(CloseDoorRoutine(data, silent: silent));
                }
                break;
        }
    }

    IEnumerator OpenDoorRoutine(ElevatorDoorData data, bool silent = false) {
        doorsAreClosed = false;
        if (!silent)
            Toolbox.RandomizeOneShot(audioSource, doorOpen);
        yield return Toolbox.Ease(null, 1f, data.closedLocalPosition.z, data.openLocalPosition.z, PennerDoubleAnimation.Linear, (amount) => {
            Vector3 newPos = new Vector3(data.openLocalPosition.x, data.openLocalPosition.y, amount);
            data.door.position = transform.TransformPoint(newPos);
        });
    }

    IEnumerator CloseDoorRoutine(ElevatorDoorData data, bool silent = false) {
        if (!silent)
            Toolbox.RandomizeOneShot(audioSource, doorCloseStart);

        yield return Toolbox.ChainCoroutines(Toolbox.Ease(null, 1f, data.openLocalPosition.z, data.closedLocalPosition.z, PennerDoubleAnimation.QuadEaseOut, (amount) => {
            Vector3 newPos = new Vector3(data.closedLocalPosition.x, data.closedLocalPosition.y, amount);
            data.door.position = transform.TransformPoint(newPos);

        }),
        new WaitForSecondsRealtime(0.1f),
        Toolbox.CoroutineFunc(() => {
            if (!silent)
                Toolbox.RandomizeOneShot(audioSource, doorCloseEnd);
        }),
        Toolbox.Ease(null, 0.6f, data.closedLocalPosition.y + 0.04f, data.closedLocalPosition.y, PennerDoubleAnimation.BounceEaseOut, (amount) => {
            Vector3 newPos = new Vector3(data.closedLocalPosition.x, amount, data.closedLocalPosition.z);
            data.door.position = transform.TransformPoint(newPos);
        }),
        Toolbox.CoroutineFunc(() => {
            doorsAreClosed = true;
        })
        );
    }

    public Coroutine ActiveCorotuine() {
        return coroutines.Values.First();
    }

    public IEnumerator WaitForDoorsToShut() {
        while (!doorsAreClosed) {
            yield return null;
        }
    }
}

