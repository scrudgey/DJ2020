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
    public TextMeshProUGUI viewCharges;
    public Action<SoftwareSelector> onSelectorChange;
    PlayerState playerState;
    public SoftwareSelector activeSelector;
    public void Initialize(PlayerState playerState) {
        this.playerState = playerState;
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
        selector.Initialize(softwareState, SelectorClickCallback);
        selector.transform.SetParent(selectorList, false);
        return selector;
    }
    public void SelectorClickCallback(SoftwareSelector selector) {
        activeSelector = selector;
        SoftwareState state = selector.softwareState;
        // set view 
        titleIcon.sprite = state.template.icon;
        viewTitle.text = state.template.name;
        viewDescription.text = "description goes here";
        viewCharges.text = "1/1";
        onSelectorChange?.Invoke(selector);
    }

}
