using System;
using System.Collections;
using System.Collections.Generic;
using Easings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class MissionSelectSoftwareView : MonoBehaviour {
    public MissionComputerController missionComputerController;

    public Transform softwareButtonContainer;
    public GameObject softwareButtonPrefab;
    public SoftwareView softwareView;

    SoftwareTemplate selectedTemplate;
    GameData data;

    public void Initialize(GameData data) {
        this.data = data;
        PopulateSoftwareList(data);
    }

    void PopulateSoftwareList(GameData data) {
        foreach (Transform child in softwareButtonContainer) {
            Destroy(child.gameObject);
        }
        selectedTemplate = null;
        foreach (SoftwareTemplate template in data.playerState.softwareTemplates) {
            GameObject obj = GameObject.Instantiate(softwareButtonPrefab);
            SoftwareSelector selector = obj.GetComponent<SoftwareSelector>();
            selector.Initialize(template, SoftwareClickCallback, null, null);
            obj.transform.SetParent(softwareButtonContainer, false);
            if (selectedTemplate == null) {
                SoftwareClickCallback(selector);
            }
        }
    }

    public void SoftwareClickCallback(SoftwareSelector button) {
        selectedTemplate = button.softwareTemplate;
        softwareView.DisplayTemplate(button.softwareTemplate);
    }
    public void CloseButtonCallback() {
        missionComputerController.ShowSoftwareListView(false);
    }
    public void DeleteButtonCallback() {
        if (selectedTemplate == null) return;
        missionComputerController.ShowModalDialogue($"Delete software {selectedTemplate.name}? This cannot be undone.", DoDelete);
    }
    public void EditButtonCallback() {
        if (selectedTemplate == null) return;
        missionComputerController.ShowSoftwareCraftViewEditMode(selectedTemplate);
    }
    public void DoDelete() {
        if (selectedTemplate == null) return;
        data.playerState.softwareTemplates.Remove(selectedTemplate);
        PopulateSoftwareList(data);
    }
}
