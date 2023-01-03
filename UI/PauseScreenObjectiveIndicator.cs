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

    public void Configure(Objective objective, GameData gameData) {
        ObjectiveStatus status = objective.Status(gameData);
        statusText.text = status switch {
            ObjectiveStatus.inProgress => "Status: incomplete",
            ObjectiveStatus.complete => "Status: complete",
            // ObjectiveStatus.disabled => "",
            ObjectiveStatus.failed => "Status: failed",
            ObjectiveStatus.canceled => "Status: canceled",
            _ => "Status: incomplete"
        };
        titleText.text = objective.title;
        descriptionText.text = objective.decsription;

        thumbnail.sprite = objective.objectiveImage;

        Color color = status switch {
            ObjectiveStatus.inProgress => inProgressColor,
            ObjectiveStatus.complete => successColor,
            ObjectiveStatus.failed => failColor,
            _ => inProgressColor
        };

        statusText.color = color;
        titleText.color = color;
    }

}
