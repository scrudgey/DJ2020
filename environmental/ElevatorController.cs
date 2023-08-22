using System.Collections;
using System.Collections.Generic;
using Easings;
using UnityEngine;

[System.Serializable]
public class ElevatorFloorData {
    public int floorNumber;
    public ElevatorIndicator indicator;
    public ElevatorDoors doors;
    public Vector3 carPosition;
}

public class ElevatorController : MonoBehaviour {
    enum State { none, move, load, close }
    State _state;
    public Transform elevatorCar;
    public ElevatorFloorData[] floors;
    Dictionary<int, ElevatorFloorData> floorDictionary;

    ElevatorFloorData targetMoveFloor;
    ElevatorFloorData currentFloor;

    public AudioSource elevatorCarAudioSource;
    public AudioClip elevatorStartSound;
    public AudioClip elevatorGoSound;
    public AudioClip elevatorStopSound;

    void Start() {
        floorDictionary = new Dictionary<int, ElevatorFloorData>();
        foreach (ElevatorFloorData data in floors) {
            floorDictionary[data.floorNumber] = data;
            Debug.Log($"closing doors {data.doors}");
            data.doors.CloseDoors();
        }

    }
    public void CallElevator(ElevatorCallButton button) {
        ElevatorFloorData data = floorDictionary[button.floorNumber];

        Debug.Log($"call elevator {data.floorNumber}");
        targetMoveFloor = data;
        ChangeState(State.move);
    }

    public void SelectFloorMove(int floorNumber) {
        ElevatorFloorData data = floorDictionary[floorNumber];

        Debug.Log($"call elevator {data.floorNumber}");
        targetMoveFloor = data;
        ChangeState(State.close);
    }


    void ChangeState(State newState) {
        State tempInitialState = _state;
        OnStateExit(tempInitialState, newState);
        _state = newState;
        OnStateEnter(tempInitialState, newState);
    }

    void OnStateExit(State oldState, State newState) {
        if (newState == oldState) return;

    }
    void OnStateEnter(State oldState, State newState) {
        // Debug.Log($"elevator doors changing state {oldState} -> {newState}");
        if (newState == oldState) return;
        switch (newState) {
            case State.move:

                if (currentFloor != null && currentFloor != targetMoveFloor) {
                    currentFloor.doors.CloseDoors();
                }

                elevatorCarAudioSource.loop = false;
                elevatorCarAudioSource.clip = elevatorStartSound;
                elevatorCarAudioSource.Play();

                float distance = Mathf.Abs(elevatorCar.position.y - targetMoveFloor.carPosition.y);
                float duration = distance;

                StartCoroutine(Toolbox.ChainCoroutines(
                    Toolbox.Ease(null, duration, elevatorCar.position.y, targetMoveFloor.carPosition.y, PennerDoubleAnimation.Linear, (amount) => {
                        Vector3 newPos = new Vector3(elevatorCar.position.x, amount, elevatorCar.position.z);
                        elevatorCar.position = newPos;
                    }),
                Toolbox.CoroutineFunc(() => {
                    elevatorCarAudioSource.Stop();
                    elevatorCarAudioSource.loop = false;
                    elevatorCarAudioSource.clip = elevatorStopSound;
                    elevatorCarAudioSource.Play();

                    currentFloor = targetMoveFloor;
                    ChangeState(State.load);
                })
                ));

                break;
            case State.load:
                currentFloor.indicator.ElevatorArrival();
                currentFloor.doors.OpenDoors();
                break;
            case State.close:
                if (currentFloor != null && currentFloor != targetMoveFloor) {
                    currentFloor.doors.CloseDoors();
                    StartCoroutine(Toolbox.ChainCoroutines(
                    currentFloor.doors.WaitForDoorsToShut(),
                    Toolbox.CoroutineFunc(() => {
                        ChangeState(State.move);
                    })
                ));
                } else {
                    ChangeState(State.load);
                }

                break;
        }
    }



    void Update() {
        if (_state == State.move) {
            if (!elevatorCarAudioSource.isPlaying) {
                elevatorCarAudioSource.loop = true;
                elevatorCarAudioSource.clip = elevatorGoSound;
                elevatorCarAudioSource.Play();
            }
        }
    }
}
