using System.Collections;
using System.Collections.Generic;
using Easings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class SoftwareModalController : MonoBehaviour {
    public SoftwareView modalView;
    public Button deployButton;
    HackTerminalController terminalController;
    SoftwareSelector activeSelector;
    void Start() {
        modalView.onSelectorChange += HandleSelectedChange;
        HandleSelectedChange(modalView.activeSelector);
    }
    void OnDestroy() {
        modalView.onSelectorChange -= HandleSelectedChange;
    }

    void HandleSelectedChange(SoftwareSelector newSelector) {
        activeSelector = newSelector;
        if (activeSelector != null) {
            deployButton.interactable = true;
        } else {
            deployButton.interactable = false; ;
        }
    }
    public void Initialize(HackTerminalController terminalController) {
        this.terminalController = terminalController;
        modalView.Initialize(GameManager.I.gameData.playerState);
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
