using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class AttackSurfaceElevatorFloorButton : AttackSurfaceElement {
    public TextMeshProUGUI buttonText;
    public TextMeshProUGUI floorText;
    public CyberElevatorController cyberElevatorController;
    public int floorNumber;
    public ElevatorFloorData floorData;
    public Color normalColor;
    public Color mouseOverColor;
    public Image buttonImage;
    public void Initialize(CyberElevatorController elevatorController, ElevatorFloorData dafloorDatata) {
        this.cyberElevatorController = elevatorController;
        this.floorData = dafloorDatata;
        Refresh();
    }
    public void OnClick() {
        cyberElevatorController.OnButtonClick(this);
    }

    public void Refresh() {
        floorText.text = $"floor {floorData.floorNumber}";
        buttonText.text = $"{floorData.lockout}";
    }

    public override void OnMouseOver() {
        buttonImage.color = mouseOverColor;
    }
    public override void OnMouseExit() {
        buttonImage.color = normalColor;
    }
    public override BurglarAttackResult HandleSingleClick(BurglarToolType activeTool, BurgleTargetData data) {
        base.HandleSingleClick(activeTool, data);
        if (activeTool == BurglarToolType.none) {
            OnClick();
        }
        return BurglarAttackResult.None;
    }
}
