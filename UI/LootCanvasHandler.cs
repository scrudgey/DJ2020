using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Easings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class LootCanvasCounter {
    public LootCategory category;
    public TextMeshProUGUI numberText;
}
public class LootCanvasHandler : MonoBehaviour {
    public RectTransform mainRect;
    public RectTransform detailsRect;
    public GraphIconReference graphIconReference;
    Coroutine showCoroutine;
    float hangTimer = 0f;

    [Header("description elements")]
    public TextMeshProUGUI lootTitle;
    public TextMeshProUGUI lootDescription;
    public TextMeshProUGUI valueText;
    public Image lootCategoryIcon;
    public Image lootImage;
    public Image creditsImage;
    public GameObject creditsContainer;
    [Header("counters")]
    public List<LootCanvasCounter> counters;
    public TextMeshProUGUI dataCounter;
    [Header("iconography")]
    public Sprite dataSprite;
    public Sprite drugSprite;
    public Sprite industrialSprite;
    public Sprite medicalSprite;
    public Sprite gemSprite;
    public Sprite commercialSprite;
    public Sprite dataPortrait;
    [Header("special loot data")]
    public LootData keyData;
    public LootData creditData;

    public void Start() {
        Bind(GameManager.I.gameData);
    }
    public void Bind(GameData data) {
        GameManager.OnLootChange += HandleLootChange;
        GameManager.OnPayDataChange += HandleDataChange;
        GameManager.OnItemPickup += HandleItemPickup;
        ConfigureLootCounts(data);
    }
    void OnDestroy() {
        GameManager.OnLootChange -= HandleLootChange;
        GameManager.OnPayDataChange -= HandleDataChange;
        GameManager.OnItemPickup -= HandleItemPickup;
    }
    public void HandleLootChange(LootData loot, GameData data) {
        ConfigureLootCounts(data);
        ConfigureLootDetails(loot, loot.isLoot);
        ShowCanvasCoroutine();
    }
    public void HandleDataChange(PayData newData, GameData data) {
        if (newData == null) return;
        ConfigureLootCounts(data);
        ConfigurePayDataDetail(newData);
        ShowCanvasCoroutine();
    }
    public void HandleItemPickup(int index, string caption) {
        LootData data = index switch {
            0 => keyData,
            1 => creditData,
        };
        ConfigureLootDetails(data, false);
        lootCategoryIcon.enabled = false;
        if (index == 0) {
            valueText.enabled = false;
        } else if (index == 1) {
            valueText.text = caption;
        }
        ShowCanvasCoroutine();
    }

    void ShowCanvasCoroutine() {
        hangTimer = 0f;
        if (showCoroutine == null) {
            showCoroutine = StartCoroutine(ShowCanvas());
        }
    }
    void ConfigureLootCounts(GameData data) {
        foreach (LootCanvasCounter counter in counters) {
            LootCategory category = counter.category;
            // foreach (LootData data in data.playerState.loots) {

            // }
            int total = data.playerState.loots
                .Where(kvp => kvp.category == category)
                .Count();
            counter.numberText.text = $"{total}";
        }
        int payDataCount = data.playerState.payDatas.Count;
        dataCounter.text = $"{payDataCount}";
    }
    void ConfigureLootDetails(LootData data, bool isLoot) {
        if (isLoot) {
            creditsContainer.SetActive(true);
            valueText.text = $"{data.GetValue()}";
            valueText.enabled = true;
            lootCategoryIcon.enabled = true;
            creditsImage.enabled = true;
            // lootCategoryIcon.sprite = data.category switch {
            //     LootCategory.drug => drugSprite,
            //     _ => dataSprite
            // };
            // lootCategoryIcon.sprite = LootTypeIcon.LootCategoryToSprite(data.category);
            lootCategoryIcon.sprite = graphIconReference.LootSprite(data.category);
        } else {
            creditsContainer.SetActive(false);
            valueText.enabled = false;
            lootCategoryIcon.enabled = false;
            creditsImage.enabled = false;
        }
        lootTitle.text = data.lootName;
        lootDescription.text = data.lootDescription;
        lootImage.sprite = data.portrait;
    }
    void ConfigurePayDataDetail(PayData data) {
        if (data == null) return;
        lootTitle.text = data.filename;
        valueText.text = $"{data.value}";
        lootImage.sprite = dataPortrait;
        lootCategoryIcon.enabled = true;
        lootCategoryIcon.sprite = dataSprite;
        lootDescription.text = "";
    }
    IEnumerator ShowCanvas() {
        float timer = 0f;
        float detailRectHeight = detailsRect.rect.height;
        mainRect.anchoredPosition = new Vector2(0f, -detailRectHeight);
        float duration = 0.5f;
        float hangtime = 4f;
        while (timer < duration) {
            timer += Time.unscaledDeltaTime;
            float y = (float)PennerDoubleAnimation.ExpoEaseOut(timer, -detailRectHeight, detailRectHeight, duration);
            mainRect.anchoredPosition = new Vector2(0, y);
            yield return null;
        }
        mainRect.anchoredPosition = new Vector2(0, 0);
        while (hangTimer < hangtime) {
            hangTimer += Time.unscaledDeltaTime;
            yield return null;
        }
        timer = 0f;
        detailRectHeight = detailsRect.rect.height;
        while (timer < duration) {
            timer += Time.unscaledDeltaTime;
            float y = (float)PennerDoubleAnimation.ExpoEaseOut(timer, 0, -detailRectHeight, duration);
            mainRect.anchoredPosition = new Vector2(0, y);
            yield return null;
        }
        detailRectHeight = detailsRect.rect.height;
        mainRect.anchoredPosition = new Vector2(0, -detailRectHeight);
        showCoroutine = null;
    }
}
