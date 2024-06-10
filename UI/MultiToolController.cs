using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MultiToolController : MonoBehaviour {
    enum State { none, usbScan, wireConnect, ramConnect }
    State state;
    // mode: tool is out, usb not selected: show prompt
    // mode: usb selected: show "scanning..."
    // mode: usb selected, mouse over: show info on connected node
    // mode: usb connected to ram chip: show decode mode
    public AudioSource audioSource;
    public AudioClip[] numberButtonTone;
    public Image screenImage;
    public TextMeshProUGUI displayText;
    [Header("state none")]
    public Sprite noneSprite;
    [Header("state scan")]
    public Sprite scanSprite;
    [Header("wire state")]
    public Sprite wireConnectSprite;
    [Header("ram state")]
    public Sprite ramConnectSprite;
    BurglarToolType currentTool;
    AttackSurfaceElement connectedElement;
    public void Initialize() {
        ChangeState(State.none);
    }
    public void MouseOverUIElementCallback(AttackSurfaceElement element) {
        if (connectedElement != null) return;
        if (currentTool == BurglarToolType.usb) {
            if (element is AttackSurfaceGraphWire) {
                ChangeState(State.wireConnect);
                DisplayWire((AttackSurfaceGraphWire)element);
            } else {
                ChangeState(State.usbScan);
            }
        } else {
            ChangeState(State.none);
        }

    }
    public void MouseExitUIElementCallback(AttackSurfaceElement element) {
        if (connectedElement != null) return;
        ChangeState(State.usbScan);
    }

    public void OnToolSelect(BurglarToolType toolType) {
        currentTool = toolType;
        if (toolType == BurglarToolType.usb) {
            connectedElement = null;
            Debug.Log($"on tool select: {toolType}");
            ChangeState(State.usbScan);
        } else {
            if (connectedElement == null)
                ChangeState(State.none);
        }
    }

    public void OnUSBToolReset() {
        connectedElement = null;
        ChangeState(State.none);
    }
    void DisplayWire(AttackSurfaceGraphWire wire) {
        string type = "";
        string targetName = "";
        if (wire.isAlarm) {
            type = "alarm network";
            targetName = GameManager.I.GetAlarmNode(wire.toId).nodeTitle;
        } else if (wire.isCyber) {
            type = "cyber network";
            targetName = GameManager.I.GetCyberNode(wire.toId).nodeTitle;
        } else if (wire.isPower) {
            type = "power network";
            targetName = GameManager.I.GetPowerNode(wire.toId).nodeTitle;
        }
        displayText.text = $"connected\ntype:{type}\nconnected to: {targetName}";
    }
    void ChangeState(State newState) {
        Debug.Log($"change state: {newState}");
        ExitState(state);
        state = newState;
        EnterState(newState);
    }
    void EnterState(State newState) {
        switch (newState) {
            case State.none:
                screenImage.sprite = noneSprite;
                displayText.text = "select tool";
                break;
            case State.usbScan:
                displayText.text = "scanning...";
                screenImage.sprite = scanSprite;
                Debug.Log($"enter usb scan state: {displayText.text}");
                break;
            case State.wireConnect:
                screenImage.sprite = wireConnectSprite;
                break;
            case State.ramConnect:
                screenImage.sprite = ramConnectSprite;
                break;
        }
    }
    void ExitState(State oldState) {
        switch (oldState) {
            case State.none:
                break;
            case State.usbScan:
                break;
            case State.wireConnect:
                break;
            case State.ramConnect:
                break;
        }
    }


    public void HandleConnection(BurglarAttackResult result) {
        if (result == null) {
            OnUSBToolReset();
            return;
        } else {
            connectedElement = result.element;
        }
    }

    public void NumericButtonCallback(int button) {
        Toolbox.RandomizeOneShot(audioSource, numberButtonTone);
    }

}
