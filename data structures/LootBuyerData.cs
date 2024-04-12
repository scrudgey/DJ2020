using System.Collections.Generic;
using System.Linq;
using UnityEngine;
[CreateAssetMenu(menuName = "ScriptableObjects/LootBuyerData")]
public class LootBuyerData : ScriptableObject {

    public Sprite portrait;
    public Sprite phonePortrait;
    public string buyerName;
    public string phoneNumber;
    public bool isMale;
    public NPCTemplate template;

    [TextArea(5, 6)]
    public string description;
    [TextArea(5, 6)]
    public string introduction;
    public List<LootPreferenceData> preferences;
    public bool isRemote;

    public float CalculateBonusFactor(LootData lootData) {
        float factor = 1f;

        List<LootPreferenceData> allPrefs = preferences
            .Concat(GameManager.I.gameData.marketData.preferences)
            .Where(preference => preference.type == lootData.category)
            .ToList();

        foreach (LootPreferenceData preference in allPrefs) {
            if (preference.type == lootData.category) {
                factor += 0.1f * preference.bonus;
            }
        }

        return factor;
    }
}

[System.Serializable]
public class LootPreferenceData {
    public LootCategory type;
    public int bonus;
    public LootPreferenceData(LootCategory type, int bonus) {
        this.type = type;
        this.bonus = bonus;
    }
}