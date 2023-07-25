using System.Collections;
using System.Collections.Generic;
using Items;
using TMPro;
using UnityEngine;
public class LoadoutStashPickerButton : MonoBehaviour {
    public enum PickerType { gun, item }
    public PickerType type;
    public MissionPlanLoadoutController loadoutController;
    public TextMeshProUGUI text;
    public GunState gunstate;
    public ItemInstance item;
    public void Initialize(MissionPlanLoadoutController controller, GunState state) {
        this.gunstate = state;
        this.loadoutController = controller;
        text.text = state.getShortName();
        type = PickerType.gun;
    }
    public void Initialize(MissionPlanLoadoutController controller, ItemInstance item) {
        this.item = item;
        this.loadoutController = controller;
        text.text = item.template.name;
        type = PickerType.item;
    }

    public void OnClick() {
        loadoutController.StashPickerCallback(this);
    }
    public void OnMouseOver() {
        loadoutController.StashPickerMouseOverCallback(this);
    }
    public void OnMouseExit() {
        loadoutController.StashPickerMouseExitCallback(this);
    }
}
