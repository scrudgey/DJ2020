using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Easings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
enum GunStat { shootInterval, noise, clipSize, spread, recoil, lockSize, damage, weight }
public class GunStatHandler : MonoBehaviour {
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI typeText;
    public TextMeshProUGUI cycleText;
    public RectTransform mainContainer;
    public Color positiveOffsetColor;
    public Color negativeOffsetColor;
    public GraphIconReference graphIconReference;

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
    // public TextMeshProUGUI recoilLowText;
    // public TextMeshProUGUI recoilHighText;

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

    [Header("perks")]
    public Transform perkIconHolder;
    public GameObject perkIconPrefab;
    public RectTransform toolTipTransform;
    public TextMeshProUGUI toolTipTitle;
    public TextMeshProUGUI toolTipDescription;

    GunStats compareGun;
    GunStats currentGun;
    Coroutine lerpBarsCoroutine;
    Dictionary<GunStat, RectTransform> statBarRects;
    Dictionary<GunStat, RectTransform> statBarOffsetRects;
    Dictionary<GunStat, float> statMaxes;
    public void SetCompareGun(IGunStatProvider compareGun) {
        if (compareGun == null) {
            this.compareGun = null;
        } else {
            this.compareGun = compareGun.GetGunStats();
        }
        if (currentGun != null) {
            LerpBars(currentGun);
        }
    }
    void Start() {
        HideToolTip();
        statBarRects = new Dictionary<GunStat, RectTransform>{
            {GunStat.shootInterval, shootIntervalBar},
            {GunStat.noise, noiseBar},
            {GunStat.clipSize, clipSizeBar},
            {GunStat.spread, spreadBar},
            {GunStat.recoil, recoilBar},
            {GunStat.lockSize, locksizeBar},
            {GunStat.damage, damageBar},
            {GunStat.weight, weightBar}
        };
        statBarOffsetRects = new Dictionary<GunStat, RectTransform>{
            {GunStat.shootInterval, shootIntervalOffsetBar},
            {GunStat.noise, noiseOffsetBar},
            {GunStat.clipSize, clipSizeOffsetBar},
            {GunStat.spread, spreadOffsetBar},
            {GunStat.recoil, recoilOffsetBar},
            {GunStat.lockSize, locksizeOffsetBar},
            {GunStat.damage, damageOffsetBar},
            {GunStat.weight, weightOffsetBar}
        };

        GunTemplate[] allTemplates = Resources.LoadAll<GunTemplate>("data/guns");
        statMaxes = new Dictionary<GunStat, float>{
            {GunStat.shootInterval, allTemplates.Select(template => template.GetGunStats().getFireRate()).Max()},
            {GunStat.noise, allTemplates.Select(template => template.GetGunStats().noise).Max()},
            {GunStat.clipSize, allTemplates.Select(template => template.GetGunStats().clipSize).Max()},
            {GunStat.spread, allTemplates.Select(template => template.GetGunStats().getAccuracy()).Max()},
            {GunStat.recoil, allTemplates.Select(template => template.GetGunStats().recoil.Average()).Max()},
            {GunStat.lockSize, allTemplates.Select(template => template.GetGunStats().lockOnSize).Max()},
            {GunStat.damage, allTemplates.Select(template => template.GetGunStats().baseDamage.Average()).Max()},
            {GunStat.weight, allTemplates.Select(template => template.GetGunStats().weight).Max()},
        };
    }
    public void DisplayGunTemplate(GunTemplate template) {
        if (template == null) {
            ClearGunTemplate();
        } else {
            SetTemplateTexts(template);
            nameText.text = template.name;
            gunImage.sprite = template.images[0];
            DisplayStats(template.GetGunStats());
            DisplayPerks(template.intrinsicPerks);
        }
    }
    public void DisplayGunState(GunState gunState) {
        if (gunState == null) {
            ClearGunTemplate();
        } else {
            SetTemplateTexts(gunState.template);
            nameText.text = gunState.getName();
            gunImage.sprite = gunState.GetSprite();
            DisplayStats(gunState.GetGunStats());
            DisplayPerks(gunState.template.intrinsicPerks.Concat(gunState.delta.perks).ToList());
        }
    }
    public void ClearGunTemplate() {
        currentGun = null;
        ClearStats();
        foreach (Transform child in perkIconHolder) {
            Destroy(child.gameObject);
        }
    }
    public void DisplayStats(GunStats newStats) {
        currentGun = newStats;
        PopulateStats(currentGun);
        LerpBars(currentGun);
    }
    public void DisplayPerks(List<GunPerk> perks) {
        foreach (Transform child in perkIconHolder) {
            Destroy(child.gameObject);
        }
        foreach (GunPerk perk in perks) {
            GameObject obj = GameObject.Instantiate(perkIconPrefab);
            obj.transform.SetParent(perkIconHolder, false);
            GunPerkIcon icon = obj.GetComponent<GunPerkIcon>();
            icon.Initialize(perk, graphIconReference, ShowToolTip, HideToolTip);
        }
    }
    void HideToolTip() {
        toolTipTransform.gameObject.SetActive(false);
    }
    void ShowToolTip(GunPerkIcon gunPerkIcon) {
        toolTipTransform.gameObject.SetActive(true);
        toolTipTransform.transform.position = gunPerkIcon.transform.position;
        toolTipTitle.text = gunPerkIcon.perk.type.ToString();
        toolTipDescription.text = gunPerkIcon.perk.Description();
    }
    void SetTemplateTexts(GunTemplate template) {
        typeText.text = template.type.ToString();
        cycleText.text = template.cycle.ToString();
    }
    public void PopulateStats(GunStats template) {
        shootIntervalText.text = (template.getFireRate()).ToString("f1");
        noiseText.text = template.noise.ToString();
        clipSizeText.text = template.clipSize.ToString();
        spreadText.text = (template.getAccuracy()).ToString("f1");

        recoilText.text = template.recoil.Average().ToString();
        // recoilLowText.text = (template.recoil.low * 10).ToString();
        // recoilHighText.text = (template.recoil.high * 10).ToString();

        lockSizeText.text = template.lockOnSize.ToString() + "m";
        damageLowText.text = template.baseDamage.low.ToString();
        damageHighText.text = template.baseDamage.high.ToString();
        weightText.text = template.weight.ToString() + "kg";
        gunImage.enabled = true;
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
        weightText.text = "";
        // recoilLowText.text = "";
        // recoilHighText.text = "";
        gunImage.enabled = false;

        foreach (GunStat stat in Enum.GetValues(typeof(GunStat))) {
            if (statBarRects != null && statBarRects.ContainsKey(stat) && statBarRects[stat] != null) {
                statBarRects[stat].sizeDelta = Vector2.zero;
                statBarOffsetRects[stat].sizeDelta = Vector2.zero;
            }
        }
    }

    public void LerpBars(GunStats template) {
        if (shootIntervalBar == null) return;
        if (lerpBarsCoroutine != null) {
            StopCoroutine(lerpBarsCoroutine);
        }
        lerpBarsCoroutine = StartCoroutine(LerpBarsRoutine(template));
    }

    public IEnumerator LerpBarsRoutine(GunStats template) {
        float duration = 1f;
        float timer = 0f;

        // Enum.GetValues(typeof(Colors))

        Dictionary<GunStat, float> initialWidths = Enum.GetValues(typeof(GunStat)).Cast<GunStat>()
            .ToDictionary(stat => stat, stat => statBarRects[stat].rect.width);

        Dictionary<GunStat, float> targetWidths = new Dictionary<GunStat, float>{
            {GunStat.clipSize, TargetBarWidth(template.clipSize, 30f, GunStat.clipSize)},
            {GunStat.damage, TargetBarWidth(template.baseDamage.Average(), 100f, GunStat.damage)},
            {GunStat.lockSize, TargetBarWidth(template.lockOnSize, 3f, GunStat.lockSize)},
            {GunStat.noise, TargetBarWidth(template.noise, 50f, GunStat.noise)},
            {GunStat.recoil, TargetBarWidth(template.recoil.Average(), 10f, GunStat.recoil)},
            {GunStat.shootInterval, TargetBarWidth(template.getFireRate() , 8f, GunStat.shootInterval)},
            {GunStat.spread, TargetBarWidth(template.getAccuracy(), 2f, GunStat.spread)},
            {GunStat.weight, TargetBarWidth(template.weight, 20f, GunStat.weight)},
        };

        // compare width should be : compare stat - gun stat -> width
        // if compare stat < 0, set x anchor to 1, else set x anchor to 0
        // 
        Dictionary<GunStat, float> initialCompareWidths = Enum.GetValues(typeof(GunStat)).Cast<GunStat>()
            .ToDictionary(stat => stat, stat => statBarOffsetRects[stat].rect.width);
        Dictionary<GunStat, float> targetCompareWidths;
        if (compareGun != null) {
            targetCompareWidths = new Dictionary<GunStat, float>{
                {GunStat.clipSize, TargetBarWidth(compareGun.clipSize - template.clipSize, 30f, GunStat.clipSize)},
                {GunStat.damage, TargetBarWidth(compareGun.baseDamage.Average() - template.baseDamage.Average(), 100f, GunStat.damage)},
                {GunStat.lockSize, TargetBarWidth(compareGun.lockOnSize - template.lockOnSize, 3f, GunStat.lockSize)},
                {GunStat.noise, TargetBarWidth(compareGun.noise - template.noise, 50f, GunStat.noise)},
                {GunStat.recoil, TargetBarWidth(compareGun.recoil.Average() - template.recoil.Average(), 10f, GunStat.recoil)},
                {GunStat.shootInterval, TargetBarWidth((compareGun.getFireRate() - template.getFireRate()), 8f, GunStat.shootInterval)},
                {GunStat.spread, TargetBarWidth(compareGun.getAccuracy() - template.getAccuracy(), 2f, GunStat.spread)},
                {GunStat.weight, TargetBarWidth(compareGun.weight - template.weight, 20f, GunStat.weight)},
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
                {GunStat.weight, 0f},
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

    float TargetBarWidth(float statValue, float maxValue, GunStat statType) {
        // statMaxes
        // float max = statMaxes[]
        float max = statMaxes[statType];
        float totalWidth = mainContainer.rect.width;
        return (statValue / max) * totalWidth;
    }
}
