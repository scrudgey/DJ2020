using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionPlanCyberController : MonoBehaviour {
    public SoftwareView softwareView;
    public Transform selectorList;
    public GameObject softwareSelectorPrefab;
    [Header("software slots")]
    public LoadoutCyberdeckSlot[] itemSlots;
    SoftwareSelector activeSelector;
    GameData data;
    LevelPlan plan;
    LevelTemplate template;
    int selectedItemSlot;

    public void Initialize(GameData data, LevelTemplate template, LevelPlan plan) {
        this.data = data;
        this.template = template;
        this.plan = plan;
        selectedItemSlot = -1;
        PopulateSoftwareList(data.playerState);
        InitializeItemSlots(plan);
    }

    void PopulateSoftwareList(PlayerState playerState) {
        foreach (Transform child in selectorList) {
            Destroy(child.gameObject);
        }

        foreach (SoftwareTemplate template in playerState.softwareTemplates) {
            SoftwareSelector newselector = CreateSoftwareSelector(template);
            if (activeSelector == null) {
                SelectorClickCallback(newselector);
            }
        }
    }
    SoftwareSelector CreateSoftwareSelector(SoftwareTemplate template) {
        GameObject obj = GameObject.Instantiate(softwareSelectorPrefab);
        SoftwareSelector selector = obj.GetComponent<SoftwareSelector>();
        selector.Initialize(template, SelectorClickCallback);
        selector.transform.SetParent(selectorList, false);
        return selector;
    }

    public void SelectorClickCallback(SoftwareSelector selector) {
        softwareView.Display(selector.softwareTemplate);
        if (selectedItemSlot != -1) {
            SetItemSlot(selectedItemSlot, selector.softwareTemplate);
            plan.softwareTemplates[selectedItemSlot] = selector.softwareTemplate;
        }
    }


    void InitializeItemSlots(LevelPlan plan) {
        for (int i = 0; i < 3; i++) {
            itemSlots[i].Initialize(this, i);
            if (i < plan.softwareTemplates.Count) {
                SetItemSlot(i, plan.softwareTemplates[i]);
            } else {
                SetItemSlot(i, null);
            }
        }
    }
    void SetItemSlot(int index, SoftwareTemplate item) {
        if (item == null) {
            itemSlots[index].Clear();
            return;
        } else {
            itemSlots[index].SetItem(item);
        }
    }
    public void SoftwareSlotClicked(int slotIndex, LoadoutCyberdeckSlot button) {
        selectedItemSlot = slotIndex;
    }
}
