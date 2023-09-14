using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class CyberElevatorController : MonoBehaviour {
    public ElevatorController elevatorController;

    public Transform buttonContainer;
    public GameObject buttonPrefab;
    public List<AttackSurfaceElevatorFloorButton> buttons;

    void Start() {
        Initialize();
    }

    public void Initialize() {
        foreach (Transform child in buttonContainer) {
            Destroy(child.gameObject);
        }
        buttons = new List<AttackSurfaceElevatorFloorButton>();
        foreach (ElevatorFloorData data in elevatorController.floors.OrderBy(floor => -1 * floor.floorNumber)) {
            GameObject obj = GameObject.Instantiate(buttonPrefab);
            obj.transform.SetParent(buttonContainer, false);
            AttackSurfaceElevatorFloorButton button = obj.GetComponent<AttackSurfaceElevatorFloorButton>();
            button.Initialize(this, data);
            buttons.Add(button);
        }
    }
    public void OnButtonClick(AttackSurfaceElevatorFloorButton button) {
        ElevatorFloorData data = button.floorData;
        // Debug.Log($"clicking {data.floorNumber}");
        switch (data.lockout) {
            case ElevatorFloorData.FloorLockoutType.locked:
                data.lockout = ElevatorFloorData.FloorLockoutType.unlocked;
                break;
            case ElevatorFloorData.FloorLockoutType.unlocked:
                data.lockout = ElevatorFloorData.FloorLockoutType.locked;
                break;
            case ElevatorFloorData.FloorLockoutType.normal:
                break;
        }
        RefreshAllButtons();
    }

    void RefreshAllButtons() {
        foreach (AttackSurfaceElevatorFloorButton button in buttons) {
            button.Refresh();
        }
    }
}
