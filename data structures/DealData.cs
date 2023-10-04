using UnityEngine;

[System.Serializable]
public record DealData {
    public LootData offerLoot;
    public Sprite offerIcon;
    public string offerName;
    public int offerCount;
    public int offerValue;

    public LootCategory priceType;
    public bool priceIsCredits;
    public int priceCount;
    public int priceValue;

    public string pitch;

    public static DealData FromLootData(LootData offer, int offerCount, LootCategory price, int priceCount, string pitch) =>
        new DealData() {
            offerLoot = offer,
            offerIcon = offer.portrait,
            offerName = offer.lootName,
            offerCount = offerCount,
            offerValue = offer.GetValue() * offerCount,

            priceType = price,
            priceIsCredits = false,
            priceCount = priceCount,
            priceValue = 0,
            pitch = pitch
        };
}