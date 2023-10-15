using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class DealButton : MonoBehaviour {
    private ImporterShopController importerShopController;
    [HideInInspector]
    public DealData dealData;

    [Header("offer")]
    public Image offerIcon;
    public TextMeshProUGUI offerTitle;
    public TextMeshProUGUI offerAmount;
    [Header("price")]
    public Image priceIcon;
    public TextMeshProUGUI priceTitle;
    public GameObject priceCreditObject;
    public TextMeshProUGUI priceAmount;
    public Sprite creditIcon;
    [Header("inventory")]
    public Image inventoryIcon;
    public TextMeshProUGUI inventoryAmountText;
    public Color notPossibleColor;

    public void Initialize(ImporterShopController importerShopController, DealData data) {
        this.importerShopController = importerShopController;
        this.dealData = data;

        offerIcon.sprite = data.offerIcon;
        offerTitle.text = $"{data.offerCount}x {data.offerName}";
        offerAmount.text = $"{data.offerValue * data.offerCount}";

        if (data.priceIsCredits) {
            priceIcon.sprite = creditIcon;
            priceCreditObject.SetActive(true);
            priceAmount.text = $"{data.priceValue}";
            priceTitle.text = "credits";

            inventoryAmountText.text = $"{GameManager.I.gameData.playerState.credits}";

            if (GameManager.I.gameData.playerState.credits < data.priceValue) {
                inventoryIcon.color = notPossibleColor;
                inventoryAmountText.color = notPossibleColor;
            }
        } else {
            priceIcon.sprite = LootTypeIcon.LootCategoryToSprite(data.priceType);
            priceCreditObject.SetActive(false);
            priceTitle.text = $"{data.priceCount}x {data.priceType}";

            int playerCount = GameManager.I.gameData.playerState.loots.Where(loot => loot.category == data.priceType).Count();
            inventoryAmountText.text = $"{playerCount}";
            if (playerCount < data.priceCount) {
                inventoryIcon.color = notPossibleColor;
                inventoryAmountText.color = notPossibleColor;
            }
        }

        // TODO: set player
        inventoryIcon.sprite = priceIcon.sprite;


    }
    public void OnClick() {
        importerShopController.OnDealButtonClick(this);
    }
}
