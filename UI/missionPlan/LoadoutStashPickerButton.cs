using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
public class LoadoutStashPickerButton : MonoBehaviour {
    public MissionPlanLoadoutController loadoutController;
    public TextMeshProUGUI text;
    public GunTemplate template;
    public void Initialize(MissionPlanLoadoutController controller, GunTemplate template) {
        this.template = template;
        this.loadoutController = controller;
        text.text = template.shortName;
    }

    public void OnClick() {
        loadoutController.StashPickerCallback(this);
    }
}
