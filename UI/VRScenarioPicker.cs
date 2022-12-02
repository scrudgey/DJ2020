using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
public class VRScenarioPicker : MonoBehaviour {
    public VRMissionDesigner missionDesigner;
    public GameObject scenarioPickerEntryPrefab;
    public Transform scenarioPickerContainer;
    public Canvas saveDialogCanvas;
    public string currentFileName;
    public TMP_InputField inputField;
    void Start() {
        saveDialogCanvas.enabled = false;
    }
    public void RefreshScenarioPicker() {
        foreach (Transform child in scenarioPickerContainer) {
            Destroy(child.gameObject);
        }
        ListAllVRMissions();
    }

    public void ScenarioPickerEntryCallback(VRScenarioPickerEntry entry) {
        missionDesigner.template = entry.template;
        missionDesigner.OnDataChange();
        currentFileName = entry.template.filename;
    }
    public void ListAllVRMissions() {
        DirectoryInfo dir = new DirectoryInfo(VRMissionTemplate.VRMissionRootDirectory());
        FileInfo[] info = dir.GetFiles("*.*");
        foreach (FileInfo f in info) {
            if (f.Name == ".DS_Store" || f.Name == VRMissionTemplate.DEFAULT_FILENAME)
                continue;
            VRMissionTemplate template = VRMissionTemplate.LoadVRMissionTemplate(f.Name);
            GameObject entryObj = GameObject.Instantiate(scenarioPickerEntryPrefab);
            entryObj.transform.SetParent(scenarioPickerContainer, false);
            entryObj.transform.localScale = Vector3.one;
            VRScenarioPickerEntry pickerEntry = entryObj.GetComponent<VRScenarioPickerEntry>();
            pickerEntry.Initialize(this, template);
        }
    }
    public void SaveButtonCallback() {
        currentFileName = missionDesigner.template.filename;
        saveDialogCanvas.enabled = true;
        TMP_InputField inputField = saveDialogCanvas.GetComponentInChildren<TMP_InputField>();
        inputField.SetTextWithoutNotify(currentFileName);
    }
    public void SaveDialogTextChanged(TMP_InputField inputField) {
        currentFileName = inputField.text;
    }
    public void SaveDialogCancelCallback() {
        saveDialogCanvas.enabled = false;
    }
    public void SaveDialogSaveCallback() {
        Debug.Log($"saving filename {currentFileName}");
        missionDesigner.template.filename = currentFileName;
        VRMissionTemplate.SaveVRMissionTemplate(missionDesigner.template, currentFileName);
        saveDialogCanvas.enabled = false;
        RefreshScenarioPicker();
    }

}
