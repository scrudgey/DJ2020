using System;
using System.Collections;
using System.Collections.Generic;
using Easings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class PayloadCraftingEntry : MonoBehaviour {
    MissionSelectSoftwareCraftController controller;
    [HideInInspector]
    public SoftwareEffect effect;
    public GraphIconReference graphIconReference;
    [Header("display")]
    public Image icon;
    public TextMeshProUGUI description;
    public TextMeshProUGUI title;
    public TextMeshProUGUI cost;
    public void Initialize(MissionSelectSoftwareCraftController controller, SoftwareEffect effect) {
        this.controller = controller;
        this.effect = effect;
        title.text = effect.TitleString();
        description.text = effect.JustDescription();
        // description.text = "payload";
        icon.sprite = graphIconReference.SoftwareEffectSprite(effect);
        cost.text = $"design: {effect.CalculateDesignPoints()}\tsize: {effect.CalculateSize()} MB";
    }

    public void DeleteCallback() {
        controller.CallbackRemovePayload(this);
    }
}
