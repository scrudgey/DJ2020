using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class PauseScreenObjectiveIndicator : MonoBehaviour {
    public Image thumbnail;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI statusText;

    public Color successColor;
    public Color failColor;
    public Color inProgressColor;

    public void Configure(ObjectiveDelta objective, GameData gameData) {
        ObjectiveStatus status = objective.status;
        statusText.text = status switch {
            ObjectiveStatus.inProgress => "Status: incomplete",
            ObjectiveStatus.complete => "Status: complete",
            // ObjectiveStatus.disabled => "",
            ObjectiveStatus.failed => "Status: failed",
            ObjectiveStatus.canceled => "Status: canceled",
            _ => "Status: incomplete"
        };
        titleText.text = objective.template.title;
        descriptionText.text = objective.template.decsription;

        thumbnail.sprite = objective.template.objectiveImage;

        Color color = status switch {
            ObjectiveStatus.inProgress => inProgressColor,
            ObjectiveStatus.complete => successColor,
            ObjectiveStatus.failed => failColor,
            _ => inProgressColor
        };

        thumbnail.color = color;
        statusText.color = color;
        titleText.color = color;
    }

}
