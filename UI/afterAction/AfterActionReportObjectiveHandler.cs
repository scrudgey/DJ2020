using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
public class AfterActionReportObjectiveHandler : MonoBehaviour {
    public TextMeshProUGUI objectiveText;
    public void Initialize(Objective objective, ObjectiveStatus status, GameData data) {
        string statusString = Objective.ReadableFinalStatus(status);
        objectiveText.text = $"{objective.title} - {statusString}";
    }
}
