using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class LootPreferenceController : MonoBehaviour {
    public TextMeshProUGUI lootTypeText;
    public LootTypeIcon[] lootIcons;
    public void Initialize(LootPreferenceData data) {
        lootTypeText.text = data.type.ToString();
        for (int i = 0; i < lootIcons.Length; i++) {
            if (i < data.bonus) {
                lootIcons[i].SetLootCategory(data.type);
                lootIcons[i].Show();
            } else {
                lootIcons[i].gameObject.SetActive(false);
            }
        }
    }
}
