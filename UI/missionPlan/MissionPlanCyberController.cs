using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Easings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MissionPlanCyberController : MonoBehaviour {
    public SoftwareView softwareView;
    [Header("cyberdeck view")]
    public Image cyberdeckArt;
    public TextMeshProUGUI cyberdeckCaption;
    [Header("selector")]
    public RectTransform selectorRectTransform;
    public Transform selectorList;
    public GameObject softwareSelectorPrefab;
    [Header("software slots")]
    public List<LoadoutCyberdeckSlot> softwareSlots;
    public List<LoadoutCyberdeckSlot> intrinsicSoftwareSlots;
    [Header("storage")]
    public TextMeshProUGUI storageText;
    public RectTransform storageBarParent;
    public RectTransform storageBarChild;
    public TextMeshProUGUI softwareListStorageText;
    public Color validColor;
    public Color invalidColor;
    SoftwareSelector activeSelector;
    GameData data;
    LevelPlan plan;
    LevelTemplate template;
    LoadoutCyberdeckSlot selectedItemSlot;
    bool softwareListIsShown;
    CyberdeckTemplate cyberdeck;
    public void Initialize(GameData data, LevelTemplate template, LevelPlan plan) {
        this.data = data;
        this.template = template;
        this.plan = plan;
        selectedItemSlot = null;
        selectorRectTransform.gameObject.SetActive(false);
        softwareListIsShown = false;
        softwareView.gameObject.SetActive(false);

        cyberdeck = data.playerState.cyberdeck;
        InitializeItemSlots(cyberdeck, plan);

        cyberdeckArt.sprite = cyberdeck.art;
        cyberdeckCaption.text = $"{cyberdeck.title}\nCAP: {cyberdeck.storageCapacity} MB\nUP: {cyberdeck.uploadSpeed}\tDown: {cyberdeck.downloadSpeed}\nslots: {cyberdeck.softwareSlots}";
    }
    void InitializeItemSlots(CyberdeckTemplate cyberdeck, LevelPlan plan) {
        int i = 0;
        foreach (LoadoutCyberdeckSlot slot in softwareSlots) {
            slot.Initialize(this);
            slot.gameObject.SetActive(i < cyberdeck.softwareSlots);
            i++;
        }
        i = 0;
        foreach (LoadoutCyberdeckSlot slot in intrinsicSoftwareSlots) {
            slot.Initialize(this);
            slot.gameObject.SetActive(i < cyberdeck.intrinsicSoftware.Count);
            i++;
        }
        for (int j = 0; j < softwareSlots.Count - 1; j++) {
            SoftwareTemplate template = j < plan.softwareTemplates.Count ? plan.softwareTemplates[j] : null;
            SetItemSlot(softwareSlots[j], template);
        }
        for (int k = 0; k < intrinsicSoftwareSlots.Count - 1; k++) {
            SoftwareTemplate template = k < cyberdeck.intrinsicSoftware.Count ? cyberdeck.intrinsicSoftware[k].ToTemplate() : null;
            intrinsicSoftwareSlots[k].SetItem(template);
        }
    }
    int TotalStorageAmount() {
        int total = 0;
        foreach (SoftwareTemplate template in plan.softwareTemplates) {
            if (template == null) continue;
            total += template.CalculateSize();
        }
        return total;
    }
    IEnumerator ShowSoftwareList(bool value) {
        softwareListIsShown = value;
        float start = value ? 35 : 550;
        float end = value ? 550 : 35;
        if (value) {
            selectorRectTransform.gameObject.SetActive(true);
        } else {
            softwareView.gameObject.SetActive(false);
        }
        SetListStorageText(null);
        SetMainStorageText(null);
        yield return Toolbox.ChainCoroutines(
             Toolbox.Ease(null, 0.1f, start, end, PennerDoubleAnimation.Linear, (amount) => {
                 selectorRectTransform.sizeDelta = new Vector2(500f, amount);
             }, unscaledTime: true),
             Toolbox.CoroutineFunc(() => {
                 if (!value) {
                     selectorRectTransform.gameObject.SetActive(false);
                 }
             })
         );
    }
    void SetListStorageText(SoftwareSelector selected) {
        int currentStorage = TotalStorageAmount();
        int maxCapacity = cyberdeck.storageCapacity;
        if (selected == null) {

        } else {
            currentStorage -= selectedItemSlot?.template?.CalculateSize() ?? 0;
            currentStorage += selected.softwareTemplate.CalculateSize();
        }
        softwareListStorageText.color = currentStorage > maxCapacity ? invalidColor : validColor;
        softwareListStorageText.text = $"storage: {currentStorage}/{maxCapacity} MB";
    }
    void SetMainStorageText(SoftwareSelector selected) {
        int currentStorage = TotalStorageAmount();
        if (selected == null) {

        } else {
            currentStorage -= selectedItemSlot?.template?.CalculateSize() ?? 0;
            currentStorage += selected.softwareTemplate.CalculateSize();
        }

        int maxCapacity = cyberdeck.storageCapacity;
        storageText.color = currentStorage > maxCapacity ? invalidColor : validColor;
        storageText.text = $"{currentStorage}/{maxCapacity} MB";

        float maxWidth = storageBarParent.rect.width;
        float percentage = (float)currentStorage / (float)maxCapacity;
        // Debug.Log($"{currentStorage}\t{maxCapacity}\t{maxWidth}\t{percentage}\t{percentage * maxWidth}");
        storageBarChild.sizeDelta = new Vector2(percentage * maxWidth, 1f);
    }
    void PopulateSoftwareList(PlayerState playerState) {
        foreach (Transform child in selectorList) {
            Destroy(child.gameObject);
        }
        foreach (SoftwareTemplate template in playerState.softwareTemplates) {
            SoftwareSelector newselector = CreateSoftwareSelector(template);
            if (activeSelector == null) {
                activeSelector = newselector;
                SelectorMouseoverCallback(newselector);
            }
        }
        SetListStorageText(null);
        SetMainStorageText(null);
    }
    SoftwareSelector CreateSoftwareSelector(SoftwareTemplate template) {
        int currentStorage = TotalStorageAmount();
        int slotSize = selectedItemSlot?.template?.CalculateSize() ?? 0;
        GameObject obj = GameObject.Instantiate(softwareSelectorPrefab);
        SoftwareSelector selector = obj.GetComponent<SoftwareSelector>();
        selector.Initialize(template, SelectorClickCallback, SelectorMouseoverCallback, SelectorMouseExitCallback);
        selector.transform.SetParent(selectorList, false);
        selector.SetInteractivility(template.CalculateSize() + currentStorage - slotSize <= cyberdeck.storageCapacity);
        return selector;
    }

    public void SelectorClickCallback(SoftwareSelector selector) {
        StartCoroutine(ShowSoftwareList(false));
        SetItemSlot(selectedItemSlot, selector.softwareTemplate);
        selectedItemSlot?.ShowHighlight(false);
    }
    public void SelectorMouseoverCallback(SoftwareSelector selector) {
        SetListStorageText(selector);
        SetMainStorageText(selector);
        softwareView.gameObject.SetActive(true);
        softwareView.DisplayTemplate(selector.softwareTemplate);
    }
    public void SelectorMouseExitCallback(SoftwareSelector selector) {
        SetListStorageText(null);
        SetMainStorageText(null);
    }

    void SetItemSlot(LoadoutCyberdeckSlot slot, SoftwareTemplate softwareTemplate) {
        if (slot == null) return;
        int index = softwareSlots.IndexOf(slot);
        softwareSlots[index].SetItem(softwareTemplate);

        // TODO: fix
        if (index < plan.softwareTemplates.Count) {
            plan.softwareTemplates[index] = softwareTemplate;
        }
        SetMainStorageText(null);
    }
    public void SoftwareSlotClicked(LoadoutCyberdeckSlot button) {
        selectedItemSlot?.ShowHighlight(false);
        button.ShowHighlight(true);
        selectedItemSlot = button;
        if (button.isIntrinsicSoftware) {
            if (softwareListIsShown) {
                ShowSoftwareList(false);
            }
            return;
        }
        PopulateSoftwareList(data.playerState);
        if (!softwareListIsShown)
            StartCoroutine(ShowSoftwareList(true));
    }
    public void SoftwareClearClicked(LoadoutCyberdeckSlot button) {
        SetItemSlot(button, null);
        if (softwareListIsShown) {
            PopulateSoftwareList(data.playerState);
        }
    }

    public void SlotMouseover(LoadoutCyberdeckSlot button) {
        if (softwareListIsShown) {

        } else {
            if (button.template != null) {
                softwareView.gameObject.SetActive(true);
                softwareView.DisplayTemplate(button.template);
            } else {
                softwareView.gameObject.SetActive(false);
                // softwareView.c
            }
        }
    }

    public void SlotMouseExit() {
        if (softwareListIsShown) {

        } else {
            softwareView.gameObject.SetActive(false);
        }
    }
}
