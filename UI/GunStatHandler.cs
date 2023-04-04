using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Easings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
enum GunStat { shootInterval, noise, clipSize, spread, recoil, lockSize, damage }//, weight }
public class GunStatHandler : MonoBehaviour {
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI typeText;
    public TextMeshProUGUI cycleText;
    public RectTransform mainContainer;
    public Color positiveOffsetColor;
    public Color negativeOffsetColor;

    [Header("shootInterval")]
    public TextMeshProUGUI shootIntervalText;
    public RectTransform shootIntervalBar;
    public RectTransform shootIntervalOffsetBar;

    [Header("noise")]
    public TextMeshProUGUI noiseText;
    public RectTransform noiseBar;
    public RectTransform noiseOffsetBar;

    [Header("clip size")]
    public TextMeshProUGUI clipSizeText;
    public RectTransform clipSizeBar;
    public RectTransform clipSizeOffsetBar;

    [Header("spread")]
    public TextMeshProUGUI spreadText;
    public RectTransform spreadBar;
    public RectTransform spreadOffsetBar;

    [Header("recoil")]
    public TextMeshProUGUI recoilText;
    public RectTransform recoilBar;
    public RectTransform recoilOffsetBar;
    public TextMeshProUGUI recoilLowText;
    public TextMeshProUGUI recoilHighText;

    [Header("locksize")]
    public TextMeshProUGUI lockSizeText;
    public RectTransform locksizeBar;
    public RectTransform locksizeOffsetBar;

    [Header("damage")]
    public TextMeshProUGUI damageLowText;
    public TextMeshProUGUI damageHighText;
    public RectTransform damageBar;
    public RectTransform damageOffsetBar;

    [Header("weight")]
    public TextMeshProUGUI weightText;
    public RectTransform weightBar;
    public RectTransform weightOffsetBar;

    public Image gunImage;

    GunTemplate compareGun;
    GunTemplate currentGun;
    Coroutine lerpBarsCoroutine;
    Dictionary<GunStat, RectTransform> statBarRects;
    Dictionary<GunStat, RectTransform> statBarOffsetRects;
    public void SetCompareGun(GunTemplate compareGun) {
        this.compareGun = compareGun;
        if (currentGun != null) {
            LerpBars(currentGun);
        }
    }
    void Start() {
        statBarRects = new Dictionary<GunStat, RectTransform>{
            {GunStat.shootInterval, shootIntervalBar},
            {GunStat.noise, noiseBar},
            {GunStat.clipSize, clipSizeBar},
            {GunStat.spread, spreadBar},
            {GunStat.recoil, recoilBar},
            {GunStat.lockSize, locksizeBar},
            {GunStat.damage, damageBar},
            // {GunStat.weight, weightBar}
        };
        statBarOffsetRects = new Dictionary<GunStat, RectTransform>{
            {GunStat.shootInterval, shootIntervalOffsetBar},
            {GunStat.noise, noiseOffsetBar},
            {GunStat.clipSize, clipSizeOffsetBar},
            {GunStat.spread, spreadOffsetBar},
            {GunStat.recoil, recoilOffsetBar},
            {GunStat.lockSize, locksizeOffsetBar},
            {GunStat.damage, damageOffsetBar},
            // {GunStat.weight, weightBar}
        };
    }
    public void DisplayGunTemplate(GunTemplate template) {
        if (template == null) {
            currentGun = null;
            ClearStats();
        } else {
            currentGun = template;
            PopulateStats(template);
            LerpBars(template);
        }
    }
    public void PopulateStats(GunTemplate template) {
        nameText.text = template.name;
        typeText.text = template.type.ToString();
        cycleText.text = template.cycle.ToString();
        shootIntervalText.text = (template.shootInterval * 10).ToString();
        noiseText.text = template.noise.ToString();
        clipSizeText.text = template.clipSize.ToString();
        spreadText.text = (template.spread * 10).ToString();
        recoilText.text = template.shootInaccuracy.ToString();
        lockSizeText.text = template.lockOnSize.ToString() + "m";
        damageLowText.text = template.baseDamage.low.ToString();
        damageHighText.text = template.baseDamage.high.ToString();
        // weightText.text = template.weight.ToString();
        recoilLowText.text = (template.recoil.low * 10).ToString();
        recoilHighText.text = (template.recoil.high * 10).ToString();
        gunImage.enabled = true;
        gunImage.sprite = template.image;
    }
    public void ClearStats() {
        nameText.text = "";
        typeText.text = "";
        cycleText.text = "";
        shootIntervalText.text = "-";
        noiseText.text = "-";
        clipSizeText.text = "-";
        spreadText.text = "-";
        recoilText.text = "-";
        lockSizeText.text = "-";
        damageLowText.text = "";
        damageHighText.text = "";
        // weightText.text = "";
        recoilLowText.text = "";
        recoilHighText.text = "";
        gunImage.enabled = false;

        foreach (GunStat stat in Enum.GetValues(typeof(GunStat))) {
            if (statBarRects != null && statBarRects.ContainsKey(stat) && statBarRects[stat] != null) {
                statBarRects[stat].sizeDelta = Vector2.zero;
                statBarOffsetRects[stat].sizeDelta = Vector2.zero;
            }
        }
    }

    public void LerpBars(GunTemplate template) {
        if (shootIntervalBar == null) return;
        if (lerpBarsCoroutine != null) {
            StopCoroutine(lerpBarsCoroutine);
        }
        lerpBarsCoroutine = StartCoroutine(LerpBarsRoutine(template));
    }

    public IEnumerator LerpBarsRoutine(GunTemplate template) {
        float duration = 1f;
        float timer = 0f;

        // Enum.GetValues(typeof(Colors))

        Dictionary<GunStat, float> initialWidths = Enum.GetValues(typeof(GunStat)).Cast<GunStat>()
            .ToDictionary(stat => stat, stat => statBarRects[stat].rect.width);

        Dictionary<GunStat, float> targetWidths = new Dictionary<GunStat, float>{
            {GunStat.clipSize, TargetBarWidth(template.clipSize, 30f)},
            {GunStat.damage, TargetBarWidth(template.baseDamage.Average(), 100f)},
            {GunStat.lockSize, TargetBarWidth(template.lockOnSize, 3f)},
            {GunStat.noise, TargetBarWidth(template.noise, 50f)},
            {GunStat.recoil, TargetBarWidth(template.recoil.Average(), 10f)},
            {GunStat.shootInterval, TargetBarWidth(template.shootInterval, 1f)},
            {GunStat.spread, TargetBarWidth(template.spread, 2f)},
            // {GunStat.weight, TargetBarWidth(template.spread, 20f)},
        };

        // compare width should be : compare stat - gun stat -> width
        // if compare stat < 0, set x anchor to 1, else set x anchor to 0
        // 
        Dictionary<GunStat, float> initialCompareWidths = Enum.GetValues(typeof(GunStat)).Cast<GunStat>()
            .ToDictionary(stat => stat, stat => statBarOffsetRects[stat].rect.width);
        Dictionary<GunStat, float> targetCompareWidths;
        if (compareGun != null) {
            targetCompareWidths = new Dictionary<GunStat, float>{
                {GunStat.clipSize, TargetBarWidth(compareGun.clipSize - template.clipSize, 30f)},
                {GunStat.damage, TargetBarWidth(compareGun.baseDamage.Average() - template.baseDamage.Average(), 100f)},
                {GunStat.lockSize, TargetBarWidth(compareGun.lockOnSize - template.lockOnSize, 3f)},
                {GunStat.noise, TargetBarWidth(compareGun.noise - template.noise, 50f)},
                {GunStat.recoil, TargetBarWidth(compareGun.recoil.Average() - template.recoil.Average(), 10f)},
                {GunStat.shootInterval, TargetBarWidth(compareGun.shootInterval - template.shootInterval, 1f)},
                {GunStat.spread, TargetBarWidth(compareGun.spread - template.spread, 2f)},
                // {GunStat.weight, TargetBarWidth(template.spread, 20f)},
            };
            foreach (GunStat stat in Enum.GetValues(typeof(GunStat))) {
                Image image = statBarOffsetRects[stat].GetComponent<Image>();
                if (targetCompareWidths[stat] > 0) {
                    statBarOffsetRects[stat].pivot = new Vector2(0f, 0.5f);
                    image.color = positiveOffsetColor;
                } else {
                    statBarOffsetRects[stat].pivot = new Vector2(1f, 0.5f);
                    image.color = negativeOffsetColor;
                }
                statBarOffsetRects[stat].anchoredPosition = Vector2.zero;
            }
        } else {
            targetCompareWidths = new Dictionary<GunStat, float>{
                {GunStat.clipSize, 0f},
                {GunStat.damage, 0f},
                {GunStat.lockSize, 0f},
                {GunStat.noise, 0f},
                {GunStat.recoil, 0f},
                {GunStat.shootInterval, 0f},
                {GunStat.spread, 0f},
                // {GunStat.weight, TargetBarWidth(template.spread, 20f)},
            };
        }

        while (timer < duration) {
            timer += Time.unscaledDeltaTime;
            foreach (GunStat stat in Enum.GetValues(typeof(GunStat))) {
                float width = (float)PennerDoubleAnimation.ExpoEaseOut(timer, initialWidths[stat], targetWidths[stat] - initialWidths[stat], duration);
                RectTransform rect = statBarRects[stat];
                rect.sizeDelta = new Vector2(width, 1f);
                rect.offsetMin = new Vector2(rect.offsetMin.x, 5f);
                rect.offsetMax = new Vector2(rect.offsetMax.x, -5f);

                width = (float)PennerDoubleAnimation.ExpoEaseOut(timer, initialCompareWidths[stat], targetCompareWidths[stat] - initialCompareWidths[stat], duration);
                rect = statBarOffsetRects[stat];
                rect.sizeDelta = new Vector2(Mathf.Abs(width), 1f);
            }
            yield return null;
        }
        foreach (GunStat stat in Enum.GetValues(typeof(GunStat))) {
            RectTransform rect = statBarRects[stat];
            rect.sizeDelta = new Vector2(targetWidths[stat], 1f);
            rect.offsetMin = new Vector2(rect.offsetMin.x, 5f);
            rect.offsetMax = new Vector2(rect.offsetMax.x, -5f);

            rect = statBarOffsetRects[stat];
            rect.sizeDelta = new Vector2(Mathf.Abs(targetCompareWidths[stat]), 1f);
        }
        lerpBarsCoroutine = null;
    }

    float TargetBarWidth(float statValue, float maxValue) {
        float totalWidth = mainContainer.rect.width;
        return (statValue / maxValue) * totalWidth;
    }
}
