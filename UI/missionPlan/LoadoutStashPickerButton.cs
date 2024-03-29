using System.Collections;
using System.Collections.Generic;
using Items;
using TMPro;
using UnityEngine;
public class LoadoutStashPickerButton : MonoBehaviour {
    public enum PickerType { gun, item, melee }
    public PickerType type;
    public MissionPlanLoadoutController loadoutController;
    public TextMeshProUGUI text;
    public WeaponState gunstate;
    public ItemTemplate item;
    public void Initialize(MissionPlanLoadoutController controller, WeaponState state) {
        this.gunstate = state;
        this.loadoutController = controller;
        text.text = state.GetShortName();
        type = PickerType.gun;
    }
    public void Initialize(MissionPlanLoadoutController controller, ItemTemplate item) {
        this.item = item;
        this.loadoutController = controller;
        text.text = item.name;
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
