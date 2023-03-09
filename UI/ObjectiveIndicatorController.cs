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
    Coroutine indicateNewRoutine;
    public Image outlineImage;
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

    public void IndicateValueChanged() {
        if (indicateNewRoutine != null) {
            StopCoroutine(indicateNewRoutine);
        }
        Color initialColor = outlineImage.color;
        indicateNewRoutine = StartCoroutine(FlashRoutine(initialColor));
    }

    IEnumerator FlashRoutine(Color initialColor) {
        // float timer = 0f;
        float interval = 0.1f;
        int index = 0;
        int cycles = 10;
        WaitForSecondsRealtime waiter = new WaitForSecondsRealtime(interval);
        while (index < cycles) {
            if (outlineImage.color == initialColor) {
                outlineImage.color = Color.white;
            } else {
                outlineImage.color = initialColor;
            }
            index += 1;
            yield return waiter;
        }
        indicateNewRoutine = null;
        outlineImage.color = initialColor;
        yield return null;
    }
}
