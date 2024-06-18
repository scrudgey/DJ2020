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

    public void SetGunsForSale() {
        GunTemplate[] allTemplates = Resources.LoadAll<GunTemplate>("data/guns") as GunTemplate[];

        List<GunState> gunsForSale = new List<GunState>();
        foreach (GunState gunState in gameData.gunsForSale) {
            if (UnityEngine.Random.Range(0f, 1f) < 0.5f) {
                gunsForSale.Add(gunState);
            }
        }

        float desiredSize = Toolbox.GetPoisson(12);
        desiredSize = Mathf.Min(desiredSize, 26f);
        int j = 0;
        Debug.Log($"desired number of guns for sale: {desiredSize}");
        while (gunsForSale.Count < desiredSize) {
            GunTemplate template = Toolbox.RandomFromListByWeight(allTemplates, (GunTemplate template) => template.likelihoodWeight);
            if (gunsForSale.Select(gun => gun.template).Where(existingGun => template == existingGun).Count() >= 2) {
                j++;
                if (j > 100) {
                    break;
                }
                continue;
            }
            GunState newGun = GunState.Instantiate(template, applyRandomPerks: true);
            gunsForSale.Add(newGun);
        }
        gameData.gunsForSale = gunsForSale;
    }


}