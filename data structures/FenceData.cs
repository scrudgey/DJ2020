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
        string pronoun = fence.isMale ? "he" : "she";
        string passivePronoun = fence.isMale ? "him" : "her";
        string informalIdentifier = fence.isMale ? "the guy" : "the girl";
        string finalName = fence.informalNickname != "" ? fence.informalNickname : fence.buyerName;

        string locationString = location switch {
            FenceLocation.alley => "the firepit",
            FenceLocation.bar => "the alibi",
            FenceLocation.importerShop => "the shop front",
            FenceLocation.nowhere => "nowhere",
            FenceLocation.popupTent => "the market stalls"
        };

        if (fence.buyerName == "Snake Man") {
            this.location = FenceLocation.alley;
            // "Word on the street is Snake's slinging his stuff by the firepit. Buyer beware, though."
            this.barLocatorDescription = "Word on the street is Snake's slinging his stuff by the firepit. Buyer beware, though.";
        } else {
            var x = Enum.GetValues(typeof(FenceLocation)).Cast<FenceLocation>().ToList();
            this.location = Toolbox.RandomFromList<FenceLocation>(x);
            if (fence.isRemote) {
                // "Haven't seen the guy. Booth in the corner has a vidphone, but I doubt it's any use."
                this.barLocatorDescription = $"Haven't seen {informalIdentifier}. Booth in the corner has a vidphone, but I doubt it's any use.";
            } else if (location != FenceLocation.nowhere) {
                // I heard a fancy fella's been seen waitin' around by the import shop.
                if (location == FenceLocation.bar) {
                    this.barLocatorDescription = $"{finalName}'s here at the Alibi, somewhere.";

                } else if (location == FenceLocation.nowhere) {
                    this.barLocatorDescription = $"{finalName} is nowhere, you dig? {finalName} ducked out for a spell. {informalIdentifier} is gone.";

                } else {
                    this.barLocatorDescription = $"I heard {finalName}'s been seen waitin' around by {locationString}.";
                }
            } else {
                // No sign of Snake today. Maybe slithered off somewhere else.
                this.barLocatorDescription = $"No sign of {finalName} today. Maybe slithered off somewhere else.";
            }
        }
    }
}
public enum FenceLocation { nowhere, alley, importerShop, popupTent, bar }