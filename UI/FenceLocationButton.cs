using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class FenceLocationButton : MonoBehaviour {
    public GameObject lootPreferencePrefab;
    public Transform buyerPreferencesContainer;
    public Image portrait;
    public TextMeshProUGUI nameText;
    public FenceData data;
    public Action<FenceLocationButton> callback;
    public void Initialize(FenceData data, Action<FenceLocationButton> callback) {
        // this.controller = controller;
        this.callback = callback;
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
    }

    public void OnClick() {
        // controller.ButtonCallback(this);
        callback?.Invoke(this);
    }
    // public void OnMouseOver() {
    //     controller.ButtonMouseOver(this);
    // }
    // public void OnMouseExit() {
    //     controller.ButtonMouseExit();
    // }
}
