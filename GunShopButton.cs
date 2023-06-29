using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class GunShopButton : MonoBehaviour {
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI costText;
    public TextMeshProUGUI innerDescriptionText;
    public Image gunImage;
    public Image creditsImage;

    [HideInInspector]
    public GunSaleData saleData;
    [HideInInspector]
    public GunState gunState;
    Action<GunShopButton> callback;
    public void Initialize(GunSaleData saleData, Action<GunShopButton> callback) {
        this.saleData = saleData;
        this.callback = callback;
        titleText.text = saleData.template.shortName;
        costText.text = saleData.cost.ToString();
        innerDescriptionText.text = saleData.template.name;
        gunImage.sprite = saleData.template.images[0];
    }
    public void Initialize(GunState state, Action<GunShopButton> callback) {
        this.gunState = state;
        this.callback = callback;
        titleText.text = state.getShortName();
        costText.text = "";
        innerDescriptionText.text = state.template.name;
        gunImage.sprite = state.GetSprite();
        creditsImage.enabled = false;
    }
    public void Clicked() {
        callback(this);
    }
}
