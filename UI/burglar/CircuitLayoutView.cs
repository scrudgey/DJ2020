using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
public class CircuitLayoutView : MonoBehaviour {
    [Header("wires")]
    public List<AttackSurfaceGraphWire> cyberWires;
    public List<AttackSurfaceGraphWire> alarmWires;
    public List<AttackSurfaceGraphWire> powerWires;

    [Header("chips")]
    public AttackSurfaceDoorLockChip doorlockChip;
    public AttackSurfaceInputChip inputChip;
    public GameObject powerLED;
    public SpriteRenderer powerLEDBackground;
    public Color ledPoweredColor;
    public Color ledUnpoweredColor;
    public GameObject lockDisplayObject;
    public TextMeshPro lockDisplayText;

    bool cyberNodeInitialized;
    bool alarmNodeInitialized;
    bool powerNodeInitialized;
    public void InitializeCyber(CyberNode node) {
        List<CyberNode> neighbors = GameManager.I.gameData.levelState.delta.cyberGraph.Neighbors(node); // 0, 1
        List<AttackSurfaceGraphWire> wires = cyberWires.OrderBy(item => Random.Range(0f, 1f)).ToList();
        for (int i = 0; i < wires.Count; i++) {
            if (i < neighbors.Count) {
                wires[i].gameObject.SetActive(true);
                wires[i].Initialize(node.idn, neighbors[i].idn);
            } else {
                wires[i].gameObject.SetActive(false);
            }
        }
        cyberNodeInitialized = true;
    }

    public void InitializeAlarm(AlarmNode node) {
        List<AlarmNode> neighbors = GameManager.I.gameData.levelState.delta.alarmGraph.Neighbors(node); // 0, 1
        List<AttackSurfaceGraphWire> wires = alarmWires.OrderBy(item => Random.Range(0f, 1f)).ToList();
        for (int i = 0; i < wires.Count; i++) {
            if (i < neighbors.Count) {
                wires[i].gameObject.SetActive(true);
                wires[i].Initialize(node.idn, neighbors[i].idn);
            } else {
                wires[i].gameObject.SetActive(false);
            }
        }
        alarmNodeInitialized = true;
    }
    public void InitializePower(PowerNode node) {
        List<PowerNode> neighbors = GameManager.I.gameData.levelState.delta.powerGraph.Neighbors(node); // 0, 1
        List<AttackSurfaceGraphWire> wires = powerWires.OrderBy(item => Random.Range(0f, 1f)).ToList();

        for (int i = 0; i < wires.Count; i++) {
            if (i < neighbors.Count) {
                wires[i].gameObject.SetActive(true);
                wires[i].Initialize(node.idn, neighbors[i].idn);
            } else {
                wires[i].gameObject.SetActive(false);
            }
        }
        powerNodeInitialized = true;
    }

    public void InitializeDoorlockChip(DoorLock doorLock) {
        doorlockChip.gameObject.SetActive(doorLock != null);
        if (doorLock != null) {
            doorlockChip.Initialize(doorLock);
        }
    }
    public void InitializeInputChip(DoorLock doorLock, ElevatorController elevatorController) {
        lockDisplayObject.gameObject.SetActive(doorLock != null);
        inputChip.gameObject.SetActive(doorLock != null);
        if (doorLock != null) {
            inputChip.Initialize(doorLock, elevatorController);
        }
    }
    void Start() {
        if (!cyberNodeInitialized) {
            foreach (AttackSurfaceGraphWire wire in cyberWires) {
                wire.gameObject.SetActive(false);
            }
        }
        if (!alarmNodeInitialized) {
            foreach (AttackSurfaceGraphWire wire in alarmWires) {
                wire.gameObject.SetActive(false);
            }
        }
        if (!powerNodeInitialized) {
            foreach (AttackSurfaceGraphWire wire in powerWires) {
                wire.gameObject.SetActive(false);
            }
        }
    }

    void Update() {
        if (inputChip.isActiveAndEnabled && inputChip.doorLock != null) {
            lockDisplayText.text = inputChip.doorLock.locked ? "door lock:\nengaged" : "door lock:\nopen";
        }
    }
}
