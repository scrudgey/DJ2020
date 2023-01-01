using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
public class AfterActionReportObjectiveHandler : MonoBehaviour {
    public TextMeshProUGUI objectiveText;
    public void Initialize(Objective objective, GameData data) {
        objectiveText.text = objective.title;
    }
}
