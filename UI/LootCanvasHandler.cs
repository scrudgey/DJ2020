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
    Coroutine showCoroutine;
    float hangTimer = 0f;

    [Header("description elements")]
    public TextMeshProUGUI lootTitle;
    public TextMeshProUGUI lootDescription;
    public TextMeshProUGUI valueText;
    public Image lootCategoryIcon;
    public Image lootImage;
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

    public void Start() {
        Bind(GameManager.I.gameData);
    }
    public void Bind(GameData data) {
        GameManager.OnLootChange += HandleLootChange;
        GameManager.OnPayDataChange += HandleDataChange;
        ConfigureLootCounts(data);
    }
    void OnDestroy() {
        GameManager.OnLootChange -= HandleLootChange;
        GameManager.OnPayDataChange -= HandleDataChange;
    }
    public void HandleLootChange(LootData loot, GameData data) {
        ConfigureLootCounts(data);
        ConfigureLootDetails(loot);
        ShowCanvasCoroutine();
    }
    public void HandleDataChange(List<PayData> newData, GameData data) {
        ConfigureLootCounts(data);
        ConfigurePayDataDetail(newData.First());
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
            int total = data.playerState.loots.ToList()
                .Where(kvp => kvp.Key.category == category)
                .Select(kvp => kvp.Value)
                .Sum();
            counter.numberText.text = $"{total}";
        }
        int payDataCount = data.playerState.payDatas.Count;
        dataCounter.text = $"{payDataCount}";
    }
    void ConfigureLootDetails(LootData data) {
        lootTitle.text = data.lootName;
        lootDescription.text = data.lootDescription;
        valueText.text = $"value: {data.value} credits";
        lootImage.sprite = data.portrait;
        lootCategoryIcon.sprite = data.category switch {
            LootCategory.drug => drugSprite,
            _ => dataSprite
        };
    }
    void ConfigurePayDataDetail(PayData data) {
        lootTitle.text = data.filename;
        valueText.text = $"value: {data.value} credits";
        lootImage.sprite = dataPortrait;
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
