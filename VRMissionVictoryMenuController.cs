using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
public class VRMissionVictoryMenuController : MonoBehaviour {
    public GameObject UIEditorCamera;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI missionTypeText;
    public TextMeshProUGUI killCounText;
    public TextMeshProUGUI timeText;
    public VRMissionState data;
    void Awake() {
        DestroyImmediate(UIEditorCamera);
    }
    public void Initialize(VRMissionState data) {
        this.data = data;
        missionTypeText.text = data.template.missionType.ToString();
        killCounText.text = data.data.numberNPCsKilled.ToString();
        SetTimeDisplay();
        SetTitle();
    }
    public void RetryButtonCallback() {
        GameManager.I.CloseMenu();
        GameManager.I.LoadVRMission(data.template);
    }
    public void MainMenuButtonCallback() {
        GameManager.I.CloseMenu();
        GameManager.I.ReturnToTitleScreen();
    }

    void SetTimeDisplay() {
        int minutes = Mathf.FloorToInt(data.data.secondsPlayed / 60F);
        int seconds = Mathf.FloorToInt(data.data.secondsPlayed - minutes * 60);
        string niceTime = string.Format("{0:0}:{1:00}", minutes, seconds);
        timeText.text = niceTime;
    }
    void SetTitle() {
        switch (data.data.status) {
            default:
            case VRMissionDelta.Status.victory:
                titleText.text = "VR Mission Success";
                break;
            case VRMissionDelta.Status.fail:
                titleText.text = "VR Mission Failed";
                break;
        }
    }
}
