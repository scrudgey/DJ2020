using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class LootInventoryButton : MonoBehaviour {
    public Image icon;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI countText;
    public LootTypeIcon[] typeIcons;
    List<LootData> lootData;
    int count;
    LootShopController lootShopController;
    public void Initialize(LootShopController lootShopController, List<LootData> datas, int count) {
        this.lootShopController = lootShopController;
        this.lootData = datas;
        this.count = count;
        LootData data = datas.First();
        nameText.text = data.lootName;
        countText.text = $"{count}";
        icon.sprite = data.portrait;
        typeIcons[0].SetLootCategory(data.category);
        typeIcons[1].Hide();
        typeIcons[2].Hide();
        typeIcons[3].Hide();
    }
    public void Clicked() {
        lootShopController.LootButtonCallback(lootData);
    }
}
