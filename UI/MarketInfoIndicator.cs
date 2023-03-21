using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class MarketInfoIndicator : MonoBehaviour {
    public LootTypeIcon lootTypeIcon;
    public Image backgroundImage;
    public Color background1;
    public Color background2;
    public TextMeshProUGUI typeNameText;
    public TextMeshProUGUI amountText;

    public void Initialize(LootPreferenceData data) {
        lootTypeIcon.SetLootCategory(data.type);
        int amount = data.bonus * 10;
        typeNameText.text = data.type.ToString();
        amountText.text = $"+{amount}%";
    }
    public void SetBackgroundColor(int index) {
        backgroundImage.color = index switch {
            1 => background1,
            2 => background2,
            _ => background1
        };
    }
}
