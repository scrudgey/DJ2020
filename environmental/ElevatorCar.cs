using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
public class ElevatorCar : MonoBehaviour {
    public TextMeshPro elevatorFloorIndicator;

    public void SetCurrentFloor(int floorNumber) {
        elevatorFloorIndicator.text = $"{floorNumber}";
    }
}
