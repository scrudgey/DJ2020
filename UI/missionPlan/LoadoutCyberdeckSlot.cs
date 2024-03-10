using System.Collections;
using System.Collections.Generic;
using Items;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadoutCyberdeckSlot : MonoBehaviour {
    public MissionPlanCyberController controller;
    public Image softwareImage;
    public SoftwareTemplate template;
    public TextMeshProUGUI softwareName;
    int index;

    public void Initialize(MissionPlanCyberController controller, int index) {
        this.controller = controller;
        this.index = index;
    }
    public void SetItem(SoftwareTemplate template) {
        if (template == null) {
            Clear();
            return;
        }
        this.template = template;
        softwareImage.enabled = true;
        softwareImage.sprite = template.icon;
        softwareName.text = template.name;
    }
    public void Clear() {
        softwareImage.enabled = false;
        softwareName.text = "";
    }
    public void OnClick() {
        controller.SoftwareSlotClicked(index, this);
    }
}
