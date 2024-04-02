using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Nimrod;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
public partial class GameManager : Singleton<GameManager> {
    public void SetMarketData() {

        MarketTemplate[] marketTemplates = Resources.LoadAll<MarketTemplate>("data/marketTemplates").Where(template => template.name != "bonus").ToArray();

        MarketTemplate template = Toolbox.RandomFromList(marketTemplates);

        gameData.marketData = template.Compile();

        if (gameData.playerState.PerkBonusMarketForces()) {
            MarketTemplate bonusTemplate = Resources.Load<MarketTemplate>("data/marketTemplates/bonus");
            MarketData bonusData = bonusTemplate.Compile();
            gameData.marketData.preferences.AddRange(bonusData.preferences);
        }
    }
    public void SetDealData() {
        DealTemplate[] dealTemplates = Resources.LoadAll<DealTemplate>("data/dealTemplates");

        List<DealData> dealDatas = new List<DealData>();

        int numberOfDeals = gameData.playerState.PerkNumberOfDailyDeals();

        for (int i = 0; i < numberOfDeals; i++) {
            DealTemplate template = Toolbox.RandomFromList(dealTemplates);
            DealData data = template.Compile();
            dealDatas.Add(data);
        }
        gameData.dealData = dealDatas;
    }
    public void SetFenceData() {
        List<FenceData> fenceDatas = new List<FenceData>();
        foreach (LootBuyerData fence in gameData.playerState.unlockedFences) {
            FenceData data = new FenceData(fence);
            fenceDatas.Add(data);
        }
        gameData.fenceData = fenceDatas;
    }
}