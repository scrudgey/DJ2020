using System;
using System.Collections;
using System.Collections.Generic;
using Easings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class SoftwareView : MonoBehaviour {
    public Transform selectorList;
    public GameObject softwareSelectorPrefab;
    [Header("view")]
    public Image titleIcon;
    public TextMeshProUGUI viewTitle;
    public TextMeshProUGUI viewDescription;
    public TextMeshProUGUI viewRequirements;
    public TextMeshProUGUI viewCharges;
    public Action<SoftwareSelector> onSelectorChange;
    PlayerState playerState;
    public SoftwareSelector activeSelector;
    CyberNode target;
    public void Initialize(PlayerState playerState, CyberNode target) {
        this.playerState = playerState;
        this.target = target;
        PopulateSoftwareList();
    }
    void PopulateSoftwareList() {
        foreach (Transform child in selectorList) {
            Destroy(child.gameObject);
        }
        foreach (SoftwareState state in playerState.softwareStates) {
            SoftwareSelector newselector = CreateSoftwareSelector(state);
            if (activeSelector == null) {
                SelectorClickCallback(newselector);
            }
        }
    }

    SoftwareSelector CreateSoftwareSelector(SoftwareState softwareState) {
        GameObject obj = GameObject.Instantiate(softwareSelectorPrefab);
        SoftwareSelector selector = obj.GetComponent<SoftwareSelector>();
        selector.Initialize(softwareState, SelectorClickCallback, target);
        selector.transform.SetParent(selectorList, false);
        return selector;
    }
    public void SelectorClickCallback(SoftwareSelector selector) {
        activeSelector = selector;
        SoftwareState state = selector.softwareState;
        // set view 
        titleIcon.sprite = state.template.icon;
        viewTitle.text = state.template.name;

        string description = "";
        foreach (SoftwareEffect effect in state.template.effects) {
            description += effect.DescriptionString() + "\n";
        }
        viewDescription.text = description;

        string requirements = "requirements:\n";
        if (target != null) {
            if (state.template.conditions.Count == 0) {
                requirements += "none";
            } else {
                foreach (SoftwareCondition condition in state.template.conditions) {
                    requirements += condition.DescriptionString(target) + "\n";
                }
            }

        }
        viewRequirements.text = requirements;

        viewCharges.text = $"charges: {state.charges}/{state.template.maxCharges}";
        onSelectorChange?.Invoke(selector);
    }

}
