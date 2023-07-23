using System.Collections;
using System.Collections.Generic;
using Nimrod;
using UnityEngine;
[CreateAssetMenu(menuName = "ScriptableObjects/DealTemplate")]
public class DealTemplate : ScriptableObject {

    [Header("offer")]
    public LootData[] offerLoots;
    public LoHi offerCountRange;
    [Header("price")]
    public LootCategory[] priceCategories;
    public LoHi priceCountRange;
    [TextArea(15, 20)]
    public string pitchTemplate;

    public DealData Compile() {
        Grammar grammar = new Grammar();
        grammar.Load("deals");

        LootData offer = Toolbox.RandomFromList(offerLoots);
        int offerCount = Mathf.RoundToInt(offerCountRange.GetRandomInsideBound());

        LootCategory price = Toolbox.RandomFromList(priceCategories);
        int priceCount = Mathf.RoundToInt(priceCountRange.GetRandomInsideBound());

        string pitch = grammar.Parse(pitchTemplate);

        return DealData.FromLootData(offer, offerCount, price, priceCount, pitch);
    }
}
