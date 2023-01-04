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
    public GunTemplate template;
    public BaseItem item;
    public void Initialize(MissionPlanLoadoutController controller, GunTemplate template) {
        this.template = template;
        this.loadoutController = controller;
        text.text = template.shortName;
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
}
