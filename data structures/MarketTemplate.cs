using System.Collections;
using System.Collections.Generic;
using Nimrod;
using UnityEngine;




[CreateAssetMenu(menuName = "ScriptableObjects/MarketTemplate")]
public class MarketTemplate : ScriptableObject {
    [TextArea(15, 20)]
    public string pitchTemplate;

    [Header("random")]
    public LootPreferenceDataTemplate[] randomPreferences;
    public LoHi numberOfRandomPreferences;
    public LootPreferenceDataTemplate[] requiredRandomPreferences;
    [Header("required")]
    public LootPreferenceData[] requiredPreferences;

    public MarketData Compile() {
        Grammar grammar = new Grammar();
        grammar.Load("markets");

        List<LootPreferenceData> preferences = new List<LootPreferenceData>();
        preferences.AddRange(requiredPreferences);

        for (int i = 0; i < Mathf.RoundToInt(numberOfRandomPreferences.GetRandomInsideBound()); i++) {
            LootPreferenceDataTemplate template = Toolbox.RandomFromList(randomPreferences);
            if (template == null) continue;
            LootPreferenceData data = template.Compile();
            preferences.Add(data);
        }

        foreach (LootPreferenceDataTemplate template in requiredRandomPreferences) {
            preferences.Add(template.Compile());
        }

        string description = grammar.Parse(pitchTemplate);
        return new MarketData() {
            description = description,
            preferences = preferences
        };
    }
}



[System.Serializable]
public class LootPreferenceDataTemplate {
    public LootCategory[] types;
    public LoHi bonusLoHi;
    public LootPreferenceData Compile() {
        LootCategory type = Toolbox.RandomFromList(types);
        int bonus = Mathf.RoundToInt(bonusLoHi.GetRandomInsideBound());
        return new LootPreferenceData(type, bonus);
    }
}