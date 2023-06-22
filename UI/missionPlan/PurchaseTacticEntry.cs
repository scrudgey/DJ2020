using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class PurchaseTacticEntry : MonoBehaviour {
    public enum Status { forSale, purchased }
    public Button myButton;
    public TextMeshProUGUI contactText;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI statusText;
    MissionPlanTacticsController controller;
    public Tactic tactic;
    public void Initialize(MissionPlanTacticsController controller, Tactic tactic, Status status) {
        this.controller = controller;
        this.tactic = tactic;
        nameText.text = $" {tactic.title}";
        contactText.text = $" {tactic.vendorName}";
        if (status == Status.forSale) {
            statusText.text = " open";
            myButton.interactable = true;
        } else {
            statusText.text = " active";
            myButton.interactable = false;
            contactText.color = Color.black;
            nameText.color = Color.black;
            statusText.color = Color.black;
        }
        // costText.text = $" {tactic.cost}";
    }

    public void SetStatus() {

    }

    public void Click() {
        controller.AvailableEntryCallback(this);
    }
}
