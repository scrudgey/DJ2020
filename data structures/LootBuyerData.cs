using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "ScriptableObjects/LootBuyerData")]
public class LootBuyerData : ScriptableObject {
    public string buyerName;
    [TextArea(5, 6)]
    public string description;
    public List<LootPreferenceData> preferences;
}

[System.Serializable]
public class LootPreferenceData {
    public LootCategory type;
    public int bonus;
}