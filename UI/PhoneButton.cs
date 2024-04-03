using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PhoneButton : MonoBehaviour {
    public GameObject lootPreferencePrefab;
    public Transform buyerPreferencesContainer;
    public Image portrait;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI numberText;
    public TextMeshProUGUI locationText;
    public FenceData data;
    PhoneMenuController controller;
    public void Initialize(FenceData data, PhoneMenuController controller) {
        this.controller = controller;
        this.data = data;
        foreach (Transform child in buyerPreferencesContainer) {
            Destroy(child.gameObject);
        }
        portrait.sprite = data.fence.portrait;
        foreach (LootPreferenceData preference in data.fence.preferences) {
            GameObject gameObject = GameObject.Instantiate(lootPreferencePrefab);
            LootPreferenceController loot = gameObject.GetComponent<LootPreferenceController>();
            loot.Initialize(preference, hideTypeText: true);
            gameObject.transform.SetParent(buyerPreferencesContainer, false);
        }
        nameText.text = data.fence.buyerName;
        numberText.text = data.fence.phoneNumber;
    }

    public void OnClick() {
        controller.ButtonCallback(this);
    }
    public void OnMouseOver() {
        controller.ButtonMouseOver(this);
    }
    public void OnMouseExit() {
        controller.ButtonMouseExit();
    }
}
