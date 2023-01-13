using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class ObjectiveIndicatorController : MonoBehaviour {
    public TextMeshProUGUI checkBoxText;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public Color successColor;
    public Color failColor;
    public Color inProgressColor;
    public Image[] images;
    public void Configure(Objective objective, GameData gameData, int index) {
        ObjectiveStatus status = objective.Status(gameData);
        checkBoxText.text = status switch {
            ObjectiveStatus.inProgress => "",
            ObjectiveStatus.complete => "+",
            // ObjectiveStatus.disabled => "",
            ObjectiveStatus.failed => "✕",
            ObjectiveStatus.canceled => "-",
            _ => checkBoxText.text
        };
        string letterIndex = index switch {
            0 => "A",
            1 => "B",
            2 => "C",
            3 => "D",
            4 => "E",
            5 => "F",
            _ => "X"
        };
        titleText.text = $"Objective {letterIndex}";
        descriptionText.text = objective.title;


        Color color = status switch {
            ObjectiveStatus.inProgress => inProgressColor,
            ObjectiveStatus.complete => successColor,
            ObjectiveStatus.failed => failColor,
            _ => inProgressColor
        };
        foreach (Image image in images) {
            image.color = color;
        }
        checkBoxText.color = color;
        descriptionText.color = color;
        titleText.color = color;
    }

}
