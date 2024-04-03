using System;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

[System.Serializable]
public class FenceData {
    [JsonConverter(typeof(ScriptableObjectJsonConverter<LootBuyerData>))]
    public LootBuyerData fence;
    public FenceLocation location;
    public string barLocatorDescription;

    public FenceData() { }
    public FenceData(LootBuyerData fence) {
        this.fence = fence;

        if (fence.buyerName == "Snake Man") {
            this.location = FenceLocation.alley;
            this.barLocatorDescription = "Snake man is at his usual spot by the firepit.";
        } else {
            var x = Enum.GetValues(typeof(FenceLocation)).Cast<FenceLocation>().ToList();
            this.location = Toolbox.RandomFromList<FenceLocation>(x);
            if (location != FenceLocation.nowhere) {
                string pronoun = fence.isMale ? "he" : "she";
                this.barLocatorDescription = $"Oh, {pronoun}'s at the {location}";
            } else {
                this.barLocatorDescription = $"I haven't seen {fence.buyerName} around today.";
            }
        }
    }
}
public enum FenceLocation { nowhere, alley, importerShop, popupTent, bar }