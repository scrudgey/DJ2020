using System.Collections;
using System.Collections.Generic;
using Easings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class SoftwareModalController : MonoBehaviour {
    public Transform selectorList;
    public GameObject softwareSelectorPrefab;
    public SoftwareView modalView;
    public Button deployButton;
    HackTerminalController terminalController;
    SoftwareSelector activeSelector;

    void HandleSelectedChange(SoftwareSelector newSelector) {
        activeSelector = newSelector;
        if (activeSelector != null) {
            deployButton.interactable = activeSelector.softwareEnabled;
        } else {
            deployButton.interactable = false; ;
        }
    }
    public void Initialize(HackTerminalController terminalController, PlayerState playerState) {
        this.terminalController = terminalController;
        CyberNode target = terminalController?.hackTarget?.node ?? null;
        CyberNode origin = terminalController?.hackOrigin?.node ?? null;
        List<CyberNode> path = terminalController.path;
        modalView.Initialize(GameManager.I.gameData.playerState, target, origin, path);
        PopulateSoftwareList(playerState, target, origin, path);
    }
    void PopulateSoftwareList(PlayerState playerState, CyberNode target, CyberNode origin, List<CyberNode> path) {
        foreach (Transform child in selectorList) {
            Destroy(child.gameObject);
        }
        foreach (SoftwareState state in playerState.softwareStates) {
            SoftwareSelector newselector = CreateSoftwareSelector(state, target, origin, path);
            if (activeSelector == null) {
                SelectorClickCallback(newselector);
            }
        }
    }
    SoftwareSelector CreateSoftwareSelector(SoftwareState softwareState, CyberNode target, CyberNode origin, List<CyberNode> path) {
        GameObject obj = GameObject.Instantiate(softwareSelectorPrefab);
        SoftwareSelector selector = obj.GetComponent<SoftwareSelector>();
        selector.Initialize(softwareState, SelectorClickCallback, target, origin, path);
        selector.transform.SetParent(selectorList, false);
        return selector;
    }
    public void SelectorClickCallback(SoftwareSelector selector) {
        modalView.DisplayState(selector.softwareState, selector.path);
        activeSelector = selector;
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
