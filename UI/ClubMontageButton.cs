using System.Collections;
using System.Collections.Generic;
using Easings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class ClubMontageButton : MonoBehaviour {
    public Image portrait;
    public TextMeshProUGUI title;
    public TextMeshProUGUI description;
    public MontageViewController montageViewController;
    public LootBuyerData lootBuyerData;
    [Header("preferences")]
    public Transform buyerPreferencesContainer;
    public GameObject lootPreferencePrefab;
    public void Initialize(MontageViewController montageViewController, LootBuyerData lootBuyerData) {
        this.montageViewController = montageViewController;
        this.lootBuyerData = lootBuyerData;
        portrait.sprite = lootBuyerData.portrait;
        title.text = lootBuyerData.buyerName;
        description.text = lootBuyerData.description;

        foreach (LootPreferenceData preference in lootBuyerData.preferences) {
            GameObject gameObject = GameObject.Instantiate(lootPreferencePrefab);
            LootPreferenceController controller = gameObject.GetComponent<LootPreferenceController>();
            controller.Initialize(preference, hideTypeText: true);
            gameObject.transform.SetParent(buyerPreferencesContainer, false);
        }
    }
    public void ClickCallback() {
        // montageViewController.BarMontageButtonCallback(this);
    }
}
