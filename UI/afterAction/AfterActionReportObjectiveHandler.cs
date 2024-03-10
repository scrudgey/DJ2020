using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
public class AfterActionReportObjectiveHandler : MonoBehaviour {
    public TextMeshProUGUI objectiveText;
    public void Initialize(ObjectiveDelta objective, ObjectiveStatus status, GameData data) {
        string statusString = Objective.ReadableFinalStatus(status);
        objectiveText.text = $"{objective.template.title} - {statusString}";
    }
}
