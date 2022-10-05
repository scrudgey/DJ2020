using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
public class VRStatHandler : MonoBehaviour {
    public TextMeshProUGUI captionTextrow1;
    public TextMeshProUGUI valueTextrow1;

    public TextMeshProUGUI captionTextrow2;
    public TextMeshProUGUI valueTextrow2;

    public GameObject row2;
    public void SetDisplay(VRMissionState state) {
        switch (state.template.missionType) {
            case VRMissionType.combat:
                captionTextrow1.text = "Kills:";
                valueTextrow1.text = $"{state.data.numberNPCsKilled} / {state.template.maxNumberNPCs}";
                row2.SetActive(false);
                break;
            case VRMissionType.steal:
                captionTextrow1.text = "Data:";
                valueTextrow1.text = $"{state.data.numberDataStoresOpened} / {state.template.targetDataCount}";
                row2.SetActive(false);
                break;
            case VRMissionType.time:
                captionTextrow1.text = "Time:";
                valueTextrow1.text = String.Format("{0:00.00}", state.template.timeLimit - state.data.secondsPlayed);
                captionTextrow2.text = "Remaining:";
                valueTextrow2.text = $"{state.template.maxNumberNPCs - state.data.numberNPCsKilled}";
                row2.SetActive(true);
                break;
            default:
                break;
        }
    }
}
