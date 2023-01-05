using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class MissionSelectorObjective : MonoBehaviour {
    public TextMeshProUGUI objectiveName;
    public TextMeshProUGUI objectiveDescription;
    public Image icon;
    public void Initialize(Objective objective) {
        objectiveName.text = objective.title;
        objectiveDescription.text = objective.decsription;
        icon.sprite = objective.objectiveImage;
    }
}
