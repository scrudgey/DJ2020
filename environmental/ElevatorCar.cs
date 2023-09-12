using System.Collections;
using System.Collections.Generic;
using Easings;
using TMPro;
using UnityEngine;
public class ElevatorCar : MonoBehaviour {
    enum State { none, open, closed }
    State _state;
    public TextMeshPro elevatorFloorIndicator;
    public TextMeshPro accessTextIndicator;
    public ElevatorDoorData[] doorDatas;
    Dictionary<ElevatorDoorData, Coroutine> coroutines;

    void Start() {
        accessTextIndicator.text = "";
    }
    public void SetCurrentFloor(int floorNumber) {
        elevatorFloorIndicator.text = $"{floorNumber}";
    }

    void InitializeCoroutines() {
        if (coroutines == null)
            coroutines = new Dictionary<ElevatorDoorData, Coroutine>();
    }
    public void OpenDoors() {
        ChangeState(State.open);
    }
    public void CloseDoors() {
        ChangeState(State.closed);
    }
    void ChangeState(State newState) {
        InitializeCoroutines();
        State tempInitialState = _state;
        OnStateExit(tempInitialState, newState);
        _state = newState;
        OnStateEnter(tempInitialState, newState);
    }

    void OnStateExit(State oldState, State newState) {
        if (newState == oldState) return;
        foreach (ElevatorDoorData data in doorDatas) {
            if (coroutines != null && coroutines.ContainsKey(data) && coroutines[data] != null) {
                StopCoroutine(coroutines[data]);
            }
        }
    }
    void OnStateEnter(State oldState, State newState) {
        if (newState == oldState) return;
        switch (newState) {
            case State.open:
                foreach (ElevatorDoorData data in doorDatas) {
                    coroutines[data] = StartCoroutine(OpenDoorRoutine(data));
                }
                break;
            case State.closed:
                foreach (ElevatorDoorData data in doorDatas) {
                    coroutines[data] = StartCoroutine(CloseDoorRoutine(data));
                }
                break;
        }
    }

    IEnumerator OpenDoorRoutine(ElevatorDoorData data) {
        yield return Toolbox.Ease(null, 1f, data.closedLocalPosition.x, data.openLocalPosition.x, PennerDoubleAnimation.Linear, (amount) => {
            Vector3 newPos = new Vector3(amount, data.openLocalPosition.y, data.openLocalPosition.z);
            data.door.localPosition = newPos;
        });
    }

    IEnumerator CloseDoorRoutine(ElevatorDoorData data) {
        yield return Toolbox.ChainCoroutines(Toolbox.Ease(null, 1f, data.openLocalPosition.x, data.closedLocalPosition.x, PennerDoubleAnimation.QuadEaseOut, (amount) => {
            Vector3 newPos = new Vector3(amount, data.closedLocalPosition.y, data.closedLocalPosition.z);
            data.door.localPosition = newPos;
        }),
        new WaitForSecondsRealtime(0.1f),
        Toolbox.Ease(null, 0.6f, data.closedLocalPosition.y + 0.04f, data.closedLocalPosition.y, PennerDoubleAnimation.BounceEaseOut, (amount) => {
            Vector3 newPos = new Vector3(data.closedLocalPosition.x, amount, data.closedLocalPosition.z);
            data.door.localPosition = newPos;
        })
        );
    }
}
