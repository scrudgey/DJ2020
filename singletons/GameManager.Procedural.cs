using System;
using System.Collections;
using System.Collections.Generic;
using Nimrod;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
public partial class GameManager : Singleton<GameManager> {
    public void SetMarketData() {

        MarketTemplate[] marketTemplates = Resources.LoadAll<MarketTemplate>("data/marketTemplates");

        MarketTemplate template = Toolbox.RandomFromList(marketTemplates);

        gameData.marketData = template.Compile();

        // gameData.marketData = new MarketData() {
        //     preferences = new List<LootPreferenceData>{
        //         new LootPreferenceData(LootCategory.drug, 1),
        //         new LootPreferenceData(LootCategory.medical, 2),
        //     },
        //     description = "big shipment of synthetic biologicals coming in from the mainland tonight."
        // };
    }
    public void SetDealData() {
        DealTemplate[] dealTemplates = Resources.LoadAll<DealTemplate>("data/dealTemplates");

        List<DealData> dealDatas = new List<DealData>();

        for (int i = 0; i < 3; i++) {
            DealTemplate template = Toolbox.RandomFromList(dealTemplates);
            DealData data = template.Compile();
            dealDatas.Add(data);
        }
        gameData.dealData = dealDatas;

        // gameData.dealData = new List<DealData>() {
        //     DealData.FromLootData(Resources.Load("data/loot/drug/rush") as LootData, 10, LootCategory.drug, 3,
        //         "Somehow I ended up with too much of this stuff. Can you take it off my hands?"),
        //     DealData.FromLootData(Resources.Load("data/loot/drug/zyme") as LootData, 3, LootCategory.drug, 2,
        //         "A big crate of zyme fell off the truck at the docks. Now I have to move it, pronto."
        //     ),
        // };
    }
}