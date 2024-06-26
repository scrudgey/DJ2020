using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Easings;
using TMPro;
using UnityEngine;

[System.Serializable]
public class ElevatorFloorData {
    public enum FloorLockoutType { normal, locked, unlocked }
    public FloorLockoutType lockout;
    public int floorNumber;
    // public ElevatorIndicator indicator;
    public ElevatorDoors doors;
    public Vector3 carPosition;
}

public class ElevatorController : MonoBehaviour {
    enum State { none, move, load, close, idle }
    State _state;
    // public Transform elevatorCar;
    public ElevatorCar elevatorCar;
    public Transform counterWeightTransform;
    public ElevatorFloorData[] floors;
    Dictionary<int, ElevatorFloorData> floorDictionary;

    ElevatorFloorData targetMoveFloor;
    ElevatorFloorData currentFloor;

    public AudioSource elevatorCarAudioSource;
    public AudioClip elevatorStartSound;
    public AudioClip elevatorGoSound;
    public AudioClip elevatorStopSound;

    private ElevatorFloorData[] floorsAscending;
    bool temporaryAuthorization;
    Coroutine temporaryAuthorizationCoroutine;
    Coroutine coroutine;

    float totalHeight;
    void Start() {
        floorDictionary = new Dictionary<int, ElevatorFloorData>();
        foreach (ElevatorFloorData data in floors) {
            floorDictionary[data.floorNumber] = data;
            // Debug.Log($"closing doors {data.doors}");
            data.doors?.CloseDoors(silent: true);
        }
        elevatorCar.CloseDoors();
        floorsAscending = floors.OrderBy(floor => floor.floorNumber).ToArray();
        // currentFloor = floors.OrderBy(floor => Mathf.Abs(floor.carPosition.y - elevatorCar.transform.position.y)).First();
        currentFloor = ClosestFloor(elevatorCar.transform.position);
        elevatorCar.transform.position = currentFloor.carPosition;
        SelectFloorMove(currentFloor.floorNumber);


        // height of counterweight = 9 - height of elevator + offset = 9 - height of elevator
        float[] heights = floors.Select(floor => floor.carPosition.y).ToArray();
        float maxHeight = heights.Max();
        float minHeight = heights.Min();
        totalHeight = maxHeight - minHeight;
        SetCounterweightPosition();
    }
    public ElevatorFloorData ClosestFloor(Vector3 position) {
        return floors.OrderBy(floor => Mathf.Abs(floor.carPosition.y - position.y)).First();
    }
    void SetCounterweightPosition() {
        counterWeightTransform.position = new Vector3(counterWeightTransform.position.x, totalHeight - elevatorCar.transform.position.y, counterWeightTransform.position.z);
    }
    public void CallElevator(ElevatorCallButton button) {
        ElevatorFloorData data = floorDictionary[button.floorNumber];

        // Debug.Log($"call elevator {data.floorNumber}");
        targetMoveFloor = data;
        ChangeState(State.move);
    }

    public void SelectFloorMove(int floorNumber) {
        ElevatorFloorData data = floorDictionary[floorNumber];
        switch (data.lockout) {
            case ElevatorFloorData.FloorLockoutType.locked:
                if (temporaryAuthorization) {
                    elevatorCar.AlertAccessGranted();
                    goto default;
                }
                elevatorCar.AlertAccessDenied();
                break;
            case ElevatorFloorData.FloorLockoutType.unlocked:
                elevatorCar.AlertAccessGranted();
                goto default;
            default:
            case ElevatorFloorData.FloorLockoutType.normal:
                targetMoveFloor = data;
                ChangeState(State.close);
                break;
        }
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
        if (coroutine != null) {
            StopCoroutine(coroutine);
            coroutine = null;
        }
        switch (newState) {
            case State.move:

                if (currentFloor != null && currentFloor != targetMoveFloor) {
                    currentFloor.doors?.CloseDoors();
                }
                elevatorCar.CloseDoors();

                elevatorCarAudioSource.loop = false;
                elevatorCarAudioSource.clip = elevatorStartSound;
                elevatorCarAudioSource.Play();

                float distance = Mathf.Abs(elevatorCar.transform.position.y - targetMoveFloor.carPosition.y);
                float duration = distance;

                StartCoroutine(
                    Toolbox.ChainCoroutines(
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
                        ChangeState(State.load);
                    }))
                );
                break;
            case State.load:
                currentFloor.doors?.elevatorIndicator.ShowLight(true);
                currentFloor.doors?.OpenDoors();
                elevatorCar.OpenDoors();
                coroutine = StartCoroutine(Toolbox.ChainCoroutines(
                    new WaitForSecondsRealtime(5f),
                    Toolbox.CoroutineFunc(() => {
                        ChangeState(State.idle);
                    })
                ));
                break;
            case State.idle:
                currentFloor.doors?.CloseDoors();
                elevatorCar.CloseDoors();
                break;
            case State.close:
                if (currentFloor != null && currentFloor != targetMoveFloor) {
                    currentFloor.doors?.CloseDoors();
                    elevatorCar.CloseDoors();
                    StartCoroutine(Toolbox.ChainCoroutines(
                        currentFloor.doors?.WaitForDoorsToShut(),
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

            // move counterweight
            SetCounterweightPosition();
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
          4 |-- <- target floor                 (10)
            |
            | 
            |
          3 |--                                 (7)
            |
            |    <- car position
            |
          2 |--  <- current floor               (4)
            |
            |
            |
          1 |--                                 (1)
            |

            total height: 10 - 1 = 9  = max(y) - min(y)
            offset: min(y)
            height of counterweight + height of elevator = 9
            height of counterweight = 9 - height of elevator + offset = 9 - height of elevator
         */


    }

    void SetCurrentFloor(ElevatorFloorData data) {
        currentFloor = data;
        currentFloor.doors?.elevatorIndicator.ElevatorArrival();
        data.doors?.callButton.AnswerCallButton();
        foreach (ElevatorFloorData floor in floors) {
            floor.doors?.elevatorIndicator.UpdateCurrentFloor(data.floorNumber);
        }
        elevatorCar.SetCurrentFloor(data.floorNumber);
    }

    public void EnableTemporaryAuthorization() {
        if (temporaryAuthorizationCoroutine != null) {
            StopCoroutine(temporaryAuthorizationCoroutine);
        }
        temporaryAuthorizationCoroutine = StartCoroutine(DoTemporaryAuthorization());
    }

    IEnumerator DoTemporaryAuthorization() {
        temporaryAuthorization = true;
        yield return new WaitForSeconds(120f);
        temporaryAuthorization = false;
    }
}
