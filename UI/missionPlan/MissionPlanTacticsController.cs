using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class MissionPlanTacticsController : MonoBehaviour {

    [Header("audio")]
    public AudioClip[] purchaseSound;
    public AudioClip[] purchaseFailSound;
    public AudioClip[] entryClickSound;
    public AudioSource audioSource;

    [Header("prefabs")]
    public GameObject availbleEntryPrefab;
    public GameObject activeEntryPrefab;

    [Header("references")]
    public Transform availableEntriesContainer;
    public Transform activeEntriesContainer;
    public Image detailsImage;
    public TextMeshProUGUI detailsTitle;
    public TextMeshProUGUI detailsText;
    public TextMeshProUGUI detailsCost;
    public TextMeshProUGUI creditsText;

    GameData data;
    PurchaseTacticEntry activeEntry;
    LevelPlan plan;
    LevelTemplate template;
    public void Initialize(GameData data, LevelTemplate template, LevelPlan plan) {
        this.data = data;
        this.template = template;
        this.plan = plan;
        SetupPurchases(template, plan);
        SetupActiveTactics(plan);
        SetCreditsText();
    }

    void SetupPurchases(LevelTemplate template, LevelPlan plan) {
        foreach (Transform child in availableEntriesContainer) {
            if (child.gameObject.name.ToLower().Contains("header")) continue;
            Destroy(child.gameObject);
        }
        List<string> activeTacticNames = plan.activeTactics.Select(tactic => tactic.title).ToList();
        foreach (Tactic tactic in template.availableTactics) {
            if (activeTacticNames.Contains(tactic.title)) continue;
            GameObject obj = Instantiate(availbleEntryPrefab);
            obj.transform.SetParent(availableEntriesContainer, false);
            PurchaseTacticEntry purchaseTacticEntry = obj.GetComponent<PurchaseTacticEntry>();
            purchaseTacticEntry.Initialize(this, tactic);
            if (activeEntry == null) {
                AvailableEntryCallback(purchaseTacticEntry);
            }
        }
    }
    void SetupActiveTactics(LevelPlan plan) {
        foreach (Transform child in activeEntriesContainer) {
            if (child.gameObject.name.ToLower().Contains("title") ||
                child.gameObject.name.ToLower().Contains("divider") ||
                child.gameObject.name.ToLower().Contains("spacer") ||
                child.gameObject.name.ToLower().Contains("credits")) continue;
            Destroy(child.gameObject);
        }
        foreach (Tactic tactic in plan.activeTactics) {
            GameObject obj = Instantiate(activeEntryPrefab);
            obj.transform.SetParent(activeEntriesContainer, false);
            ActiveTacticView activeTacticView = obj.GetComponent<ActiveTacticView>();
            activeTacticView.Initialize(tactic);
        }
    }

    public void AvailableEntryCallback(PurchaseTacticEntry entry) {
        Toolbox.RandomizeOneShot(audioSource, entryClickSound);
        ConfigurePurchaseDialogue(entry.tactic);
        activeEntry = entry;
    }
    void SetCreditsText() {
        creditsText.text = $"{data.playerState.credits}";
    }
    void ConfigurePurchaseDialogue(Tactic tactic) {
        detailsTitle.text = tactic.title;
        detailsText.text = tactic.decsription;
        detailsCost.text = tactic.cost.ToString();
        detailsImage.sprite = tactic.icon;
        detailsImage.enabled = true;
    }
    void ClearPurchaseDialogue() {
        detailsTitle.text = "";
        detailsText.text = "";
        detailsCost.text = "-";
        detailsImage.enabled = false;
    }
    public void PurchaseCallback() {
        if (activeEntry == null) {
            PurchaseFail();
            return;
        }
        if (activeEntry.tactic.cost > data.playerState.credits) {
            PurchaseFail();
        } else {
            PurchaseSuccess(activeEntry.tactic);
        }
    }

    void PurchaseSuccess(Tactic tactic) {
        Toolbox.RandomizeOneShot(audioSource, purchaseSound);
        data.playerState.credits -= tactic.cost;
        plan.activeTactics.Add(tactic);
        SetupPurchases(template, plan);
        SetupActiveTactics(plan);
        SetCreditsText();
        ClearPurchaseDialogue();
    }
    void PurchaseFail() {
        Toolbox.RandomizeOneShot(audioSource, purchaseFailSound);
    }
}
