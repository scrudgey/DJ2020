using System;
using System.Collections;
using System.Collections.Generic;
using Items;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class ItemShopButton : MonoBehaviour {
    public TextMeshProUGUI titleText;
    [HideInInspector]
    public ItemSaleData saleData;
    public BaseItem item;
    Action<ItemShopButton> callback;
    public void Initialize(ItemSaleData saleData, Action<ItemShopButton> callback) {
        this.saleData = saleData;
        this.callback = callback;
        titleText.text = saleData.item.data.shortName;
    }
    public void Initialize(BaseItem item, Action<ItemShopButton> callback) {
        this.item = item;
        this.callback = callback;
        titleText.text = item.data.shortName;
    }
    public void Clicked() {
        callback(this);
    }
}
