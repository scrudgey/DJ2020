using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class LootInventoryButton : MonoBehaviour {
    public Image icon;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI countText;
    public LootTypeIcon[] typeIcons;
    LootData lootData;
    int count;
    LootShopController lootShopController;
    public void Initialize(LootShopController lootShopController, LootData data, int count) {
        this.lootShopController = lootShopController;
        this.lootData = data;
        this.count = count;
        nameText.text = data.lootName;
        countText.text = $"{count}";
        icon.sprite = data.portrait;
        typeIcons[0].SetLootCategory(data.category);
        typeIcons[1].Hide();
        typeIcons[2].Hide();
        typeIcons[3].Hide();
    }
    public void Clicked() {
        lootShopController.LootButtonCallback(lootData, count);
    }
}
