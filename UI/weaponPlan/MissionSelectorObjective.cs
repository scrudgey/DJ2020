using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
public class MissionSelectorObjective : MonoBehaviour {
    public TextMeshProUGUI objectiveName;
    public TextMeshProUGUI objectiveDescription;
    public void Initialize(Objective objective) {
        objectiveName.text = objective.title;
        objectiveDescription.text = objective.decsription;
    }
}
