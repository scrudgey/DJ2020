using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Easings;
using TMPro;
using UnityEngine;

[System.Serializable]
public class ElevatorFloorData {
    public int floorNumber;
    // public ElevatorIndicator indicator;
    public ElevatorDoors doors;
    public Vector3 carPosition;
}

public class ElevatorController : MonoBehaviour {
    enum State { none, move, load, close }
    State _state;
    // public Transform elevatorCar;
    public ElevatorCar elevatorCar;
    public ElevatorFloorData[] floors;
    Dictionary<int, ElevatorFloorData> floorDictionary;

    ElevatorFloorData targetMoveFloor;
    ElevatorFloorData currentFloor;

    public AudioSource elevatorCarAudioSource;
    public AudioClip elevatorStartSound;
    public AudioClip elevatorGoSound;
    public AudioClip elevatorStopSound;

    private ElevatorFloorData[] floorsAscending;

    void Start() {
        floorDictionary = new Dictionary<int, ElevatorFloorData>();
        foreach (ElevatorFloorData data in floors) {
            floorDictionary[data.floorNumber] = data;
            Debug.Log($"closing doors {data.doors}");
            data.doors.CloseDoors();
        }
        floorsAscending = floors.OrderBy(floor => floor.floorNumber).ToArray();
        currentFloor = floors.OrderBy(floor => Mathf.Abs(floor.carPosition.y - elevatorCar.transform.position.y)).First();
        SelectFloorMove(currentFloor.floorNumber);
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

                float distance = Mathf.Abs(elevatorCar.transform.position.y - targetMoveFloor.carPosition.y);
                float duration = distance;

                StartCoroutine(Toolbox.ChainCoroutines(
                    Toolbox.Ease(null, duration, elevatorCar.transform.position.y, targetMoveFloor.carPosition.y, PennerDoubleAnimation.QuadEaseInOut, (amount) => {
                        Vector3 newPos = new Vector3(elevatorCar.transform.position.x, amount, elevatorCar.transform.position.z);
                        elevatorCar.transform.position = newPos;
                    }),
                    new WaitForSecondsRealtime(0.1f),
                    Toolbox.Ease(null, 0.4f, targetMoveFloor.carPosition.y + 0.05f, targetMoveFloor.carPosition.y, PennerDoubleAnimation.BounceEaseOut, (amount) => {
                        Vector3 newPos = new Vector3(elevatorCar.transform.position.x, amount, elevatorCar.transform.position.z);
                        elevatorCar.transform.position = newPos;
                    }),
                    new WaitForSecondsRealtime(1f),
                Toolbox.CoroutineFunc(() => {
                    elevatorCarAudioSource.Stop();
                    elevatorCarAudioSource.loop = false;
                    elevatorCarAudioSource.clip = elevatorStopSound;
                    elevatorCarAudioSource.Play();

                    // SetCurrentFloor(targetMoveFloor);
                    ChangeState(State.load);
                })
                ));

                break;
            case State.load:
                currentFloor.doors.elevatorIndicator.ShowLight(true);
                currentFloor.doors.OpenDoors();
                break;
            case State.close:
                if (currentFloor != null && currentFloor != targetMoveFloor) {
                    currentFloor.doors.CloseDoors();
                    StartCoroutine(Toolbox.ChainCoroutines(
                        currentFloor.doors.WaitForDoorsToShut(),
                        new WaitForSecondsRealtime(1f),
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
        if (currentFloor != null && targetMoveFloor != null && floorsAscending != null) {
            if (currentFloor.floorNumber > targetMoveFloor.floorNumber) {
                foreach (ElevatorFloorData data in floorsAscending.Reverse()) {
                    if (data.carPosition.y >= currentFloor.carPosition.y) {
                        continue;
                    }
                    if (data.carPosition.y > elevatorCar.transform.position.y) {
                        SetCurrentFloor(data);
                        break;
                    }
                }
            } else if (currentFloor.floorNumber < targetMoveFloor.floorNumber) {
                foreach (ElevatorFloorData data in floorsAscending) {
                    if (data.carPosition.y <= currentFloor.carPosition.y) {
                        continue;
                    }
                    if (data.carPosition.y < elevatorCar.transform.position.y) {
                        SetCurrentFloor(data);
                        break;
                    }
                }
            }
        }

        /**
            |
          4 |-- <- target floor
            |
            | 
            |
          3 |--
            |
            |    <- car position
            |
          2 |--  <- current floor
            |
            |
            |
          1 |-- 
            |
         */


    }

    void SetCurrentFloor(ElevatorFloorData data) {
        currentFloor = data;
        currentFloor.doors.elevatorIndicator.ElevatorArrival();
        data.doors.callButton.AnswerCallButton();
        foreach (ElevatorFloorData floor in floors) {
            floor.doors.elevatorIndicator.UpdateCurrentFloor(data.floorNumber);
        }
        elevatorCar.SetCurrentFloor(data.floorNumber);
    }
}
