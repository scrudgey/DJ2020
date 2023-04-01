using System;
using System.Collections;
using System.Collections.Generic;
using Items;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class ItemShopButton : MonoBehaviour {
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI costText;
    public Image icon;
    public Image creditIcon;
    [HideInInspector]
    public ItemSaleData saleData;
    public BaseItem item;
    Action<ItemShopButton> callback;
    public void Initialize(ItemSaleData saleData, Action<ItemShopButton> callback) {
        this.saleData = saleData;
        this.callback = callback;
        titleText.text = saleData.item.data.shortName;
        costText.text = saleData.cost.ToString();
        icon.sprite = saleData.item.data.image;
    }
    public void Initialize(BaseItem item, Action<ItemShopButton> callback) {
        this.item = item;
        this.callback = callback;
        titleText.text = item.data.shortName;
        // costText.text = saleData.cost.ToString();
        icon.sprite = item.data.image;
        costText.enabled = false;
        creditIcon.enabled = false;
    }
    public void Clicked() {
        callback(this);
    }
}
