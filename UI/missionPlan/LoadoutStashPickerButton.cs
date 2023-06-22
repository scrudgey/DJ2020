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
    public BaseItem item;
    public void Initialize(MissionPlanLoadoutController controller, GunState gunstate) {
        this.gunstate = gunstate;
        this.loadoutController = controller;
        text.text = gunstate.template.shortName;
        type = PickerType.gun;
    }
    public void Initialize(MissionPlanLoadoutController controller, BaseItem item) {
        this.item = item;
        this.loadoutController = controller;
        text.text = item.data.name;
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
