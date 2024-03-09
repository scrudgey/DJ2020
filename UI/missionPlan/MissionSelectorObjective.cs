using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class MissionSelectorObjective : MonoBehaviour {
    public TextMeshProUGUI objectiveName;
    public TextMeshProUGUI objectiveDescription;
    public TextMeshProUGUI bonusText;
    public TextMeshProUGUI statusText;
    public Image icon;


    public Color successColor;
    public Color failColor;
    public Color inProgressColor;
    public void Initialize(Objective objective, bool isBonus = false) {
        objectiveName.text = objective.title;
        objectiveDescription.text = objective.decsription;
        icon.sprite = objective.objectiveImage;
        if (isBonus) {
            bonusText.gameObject.SetActive(true);
            if (objective.bonusRewardCredits > 0) {
                bonusText.text = $"+{objective.bonusRewardCredits} credits";
            } else if (objective.bonusRewardFavors > 0) {
                bonusText.text = $"+{objective.bonusRewardFavors} favor";
            } else if (objective.bonusRewardSkillpoints > 0) {
                bonusText.text = $"+{objective.bonusRewardFavors} skill point";
            } else {
                bonusText.gameObject.SetActive(false);
            }
        } else {
            bonusText.gameObject.SetActive(false);
        }
        statusText.gameObject.SetActive(false);
    }
    public void Initialize(ObjectiveDelta delta, bool isBonus = false) {
        objectiveName.text = delta.template.title;
        objectiveDescription.text = delta.template.decsription;
        icon.sprite = delta.template.objectiveImage;
        if (isBonus) {
            bonusText.gameObject.SetActive(true);
            if (delta.template.bonusRewardCredits > 0) {
                bonusText.text = $"+{delta.template.bonusRewardCredits} credits";
            } else if (delta.template.bonusRewardFavors > 0) {
                bonusText.text = $"+{delta.template.bonusRewardFavors} favor";
            } else if (delta.template.bonusRewardSkillpoints > 0) {
                bonusText.text = $"+{delta.template.bonusRewardFavors} skill point";
            } else {
                bonusText.gameObject.SetActive(false);
            }
        } else {
            bonusText.gameObject.SetActive(false);
        }

        statusText.gameObject.SetActive(true);
        ObjectiveStatus status = delta.status;
        statusText.text = status switch {
            ObjectiveStatus.inProgress => "Status: incomplete",
            ObjectiveStatus.complete => "Status: complete",
            // ObjectiveStatus.disabled => "",
            ObjectiveStatus.failed => "Status: failed",
            ObjectiveStatus.canceled => "Status: canceled",
            _ => "Status: incomplete"
        };
        Color color = status switch {
            ObjectiveStatus.inProgress => inProgressColor,
            ObjectiveStatus.complete => successColor,
            ObjectiveStatus.failed => failColor,
            _ => inProgressColor
        };

        icon.color = color;
        statusText.color = color;
        objectiveName.color = color;
    }
}
