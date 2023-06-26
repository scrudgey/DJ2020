using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LootCounter : MonoBehaviour {
    public TextMeshProUGUI counter;
    public LootTypeIcon lootTypeIcon;
    public void Initialize(LootCategory lootCategory, int count) {
        lootTypeIcon.SetLootCategory(lootCategory);
        counter.text = $"{count}";
    }
}
