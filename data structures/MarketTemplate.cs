using System.Collections;
using System.Collections.Generic;
using Nimrod;
using UnityEngine;




[CreateAssetMenu(menuName = "ScriptableObjects/MarketTemplate")]
public class MarketTemplate : ScriptableObject {
    [TextArea(15, 20)]
    public string pitchTemplate;

    [Header("random")]
    public LoHi numberOfRandomPreferences;
    public LootPreferenceDataTemplate[] randomPreferences;
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

        string description;

        Grammar marketGrammar = new Grammar();
        marketGrammar.Load("markets");
        marketGrammar.AddSymbol("name", GameManager.I.gameData.filename);
        // string pronoun = fence.isMale ? "he" : "she";
        // string passivePronoun = fence.isMale ? "him" : "her";
        // string informalIdentifier = fence.isMale ? "the guy" : "the girl";

        if (preferences.Count == 0) {
            description = marketGrammar.Parse("{generic}");
        } else if (preferences.Count == 1) {
            LootPreferenceData data = preferences[0];

            SetGrammarKeyForLootPreference(marketGrammar, data, "1");

            if (data.bonus > 0) {
                marketGrammar.SetSymbol("pattern", "{one-up}");
            } else if (data.bonus < 0) {
                marketGrammar.SetSymbol("pattern", "{one-down}");
            }

            description = marketGrammar.Parse("{prompt-1}");
        } else {
            LootPreferenceData data1 = preferences[0];
            LootPreferenceData data2 = preferences[1];

            SetGrammarKeyForLootPreference(marketGrammar, data1, "1");
            SetGrammarKeyForLootPreference(marketGrammar, data2, "2");

            if (data1.bonus > 0) {
                marketGrammar.SetSymbol("pattern-1", "{one-up}");
            } else {
                marketGrammar.SetSymbol("pattern-1", "{one-down}");
            }

            if (data1.bonus > 0 && data2.bonus > 0) {
                marketGrammar.SetSymbol("pattern", "{two-up}");
            } else if (data1.bonus < 0 && data2.bonus < 0) {
                marketGrammar.SetSymbol("pattern", "{two-down}");
            } else {
                marketGrammar.SetSymbol("pattern", "{one-up-one-down}");
            }

            description = marketGrammar.Parse("{prompt-2}");
        }

        return new MarketData() {
            description = description,
            preferences = preferences
        };
    }

    void SetGrammarKeyForLootPreference(Grammar marketGrammar, LootPreferenceData data, string suffix) {
        string lootname = data.type switch {
            LootCategory.commercial => "commercial",
            LootCategory.drug => "drug",
            LootCategory.gem => "gem",
            LootCategory.industrial => "industrial",
            LootCategory.medical => "medical",
            LootCategory.none => "none",
        };

        marketGrammar.SetSymbol($"type-{suffix}", $"{{{lootname}}}");

        if (data.bonus > 0) {
            marketGrammar.SetSymbol($"uptype", $"{{{lootname}}}");
            marketGrammar.SetSymbol($"up-reason", $"{{{lootname}-up-reason}}");

            marketGrammar.SetSymbol($"uptype-{suffix}", $"{{{lootname}}}");
            marketGrammar.SetSymbol($"up-reason-{suffix}", $"{{{lootname}-up-reason}}");
        } else {
            marketGrammar.SetSymbol($"downtype", $"{{{lootname}}}");
            marketGrammar.SetSymbol($"down-reason", $"{{{lootname}-down-reason}}");

            marketGrammar.SetSymbol($"downtype-{suffix}", $"{{{lootname}}}");
            marketGrammar.SetSymbol($"down-reason-{suffix}", $"{{{lootname}-down-reason}}");
        }
    }
}



[System.Serializable]
public class LootPreferenceDataTemplate {
    public LootCategory[] types;
    public LoHi bonusLoHi;
    public LootPreferenceData Compile() {
        LootCategory type = Toolbox.RandomFromList(types);
        int bonus = Mathf.RoundToInt(bonusLoHi.GetRandomInsideBound());
        int bonuses = GameManager.I.gameData.playerState.PerkBetterMarketConditionsChances();
        if (bonuses > 0) {
            for (int i = 0; i < bonuses; i++) {
                if (Random.Range(0f, 1f) > 0.5f) {
                    bonus += 1;
                }
            }
        }
        return new LootPreferenceData(type, bonus);
    }
}