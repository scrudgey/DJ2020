using System.Collections;
using System.Collections.Generic;
using Easings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class SoftwareModalController : MonoBehaviour {
    public Transform selectorList;
    public GameObject softwareSelectorPrefab;
    public SoftwareView softwareView;
    public Button deployButton;
    HackTerminalController terminalController;
    SoftwareSelector activeSelector;
    List<CyberNode> path;

    public void Initialize(HackTerminalController terminalController, List<SoftwareState> softwareStates) {
        this.terminalController = terminalController;
        CyberNode target = terminalController?.hackTarget?.node ?? null;
        CyberNode origin = terminalController?.hackOrigin?.node ?? null;
        path = terminalController.path;
        softwareView.Initialize(target, origin, path);
        PopulateSoftwareList(softwareStates, target, origin);
    }
    void PopulateSoftwareList(List<SoftwareState> softwareStates, CyberNode target, CyberNode origin) {
        foreach (Transform child in selectorList) {
            Destroy(child.gameObject);
        }
        foreach (SoftwareState state in softwareStates) {
            SoftwareSelector newselector = CreateSoftwareSelector(state, target, origin);
            if (activeSelector == null) {
                SelectorClickCallback(newselector);
            }
        }
    }
    SoftwareSelector CreateSoftwareSelector(SoftwareState softwareState, CyberNode target, CyberNode origin) {
        GameObject obj = GameObject.Instantiate(softwareSelectorPrefab);
        SoftwareSelector selector = obj.GetComponent<SoftwareSelector>();
        bool softwareEnabled = false;
        if (target != null) {
            softwareEnabled = softwareState.EvaluateCondition(target, origin, path) && softwareState.charges > 0;
        }
        selector.Initialize(softwareState, SelectorClickCallback, softwareEnabled);
        selector.transform.SetParent(selectorList, false);
        return selector;
    }
    public void SelectorClickCallback(SoftwareSelector selector) {
        softwareView.DisplayState(selector.softwareState);
        activeSelector = selector;
        deployButton.interactable = activeSelector != null && activeSelector.softwareEnabled;
        // Debug.Log($"{activeSelector} {activeSelector.softwareEnabled} {deployButton.interactable}");
    }
    public void CancelCalback() {
        GameManager.I.CloseMenu();
    }
    public void DeployCallback() {
        GameManager.I.CloseMenu();
        if (activeSelector != null) {
            terminalController.DeploySoftware(activeSelector.softwareState);
        }
    }
}
