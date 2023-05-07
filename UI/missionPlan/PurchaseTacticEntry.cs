using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class PurchaseTacticEntry : MonoBehaviour {
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI costText;
    MissionPlanTacticsController controller;
    public Tactic tactic;
    public void Initialize(MissionPlanTacticsController controller, Tactic tactic) {
        this.controller = controller;
        this.tactic = tactic;
        nameText.text = $" {tactic.title}";
        costText.text = $" {tactic.cost}";
    }

    public void Click() {
        controller.AvailableEntryCallback(this);
    }
}
