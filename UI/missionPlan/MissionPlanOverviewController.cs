using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class MissionPlanOverviewController : MonoBehaviour {
    public TextMeshProUGUI descriptionText;

    [Header("intelBoxes")]
    public GameObject guardSummaryBox;
    public GameObject physicalSummaryBox;
    public GameObject securitySummaryBox;
    public TextMeshProUGUI guardSummaryText;
    public TextMeshProUGUI guardDescText;
    public TextMeshProUGUI physicalSummaryText;
    public TextMeshProUGUI physicalDescText;
    public TextMeshProUGUI securitySummaryText;
    public TextMeshProUGUI securityDescText;
    [Header("objectives")]
    public Transform objectivesContainer;
    public Transform bonusObjectivesContainer;
    public GameObject objectivePrefab;
    public GameObject bonusTitle;

    GameData data;
    LevelPlan plan;
    LevelTemplate template;

    public void Initialize(GameData data, LevelTemplate template, LevelPlan plan) {
        this.data = data;
        this.template = template;
        this.plan = plan;
        InitializeObjectives(template);
        InitializeIntel(template);
    }
    void InitializeObjectives(LevelTemplate template) {
        foreach (Transform child in objectivesContainer) {
            Destroy(child.gameObject);
        }
        foreach (Transform child in bonusObjectivesContainer) {
            Destroy(child.gameObject);
        }
        bonusTitle.SetActive(false);

        foreach (Objective objective in template.objectives) {
            GameObject obj = GameObject.Instantiate(objectivePrefab);
            obj.transform.SetParent(objectivesContainer, false);
            MissionSelectorObjective handler = obj.GetComponent<MissionSelectorObjective>();
            handler.Initialize(objective);
        }

        if (template.bonusObjectives.Count > 0) {
            bonusTitle.SetActive(true);
            // populate list
            foreach (Objective objective in template.bonusObjectives) {
                GameObject obj = GameObject.Instantiate(objectivePrefab);
                obj.transform.SetParent(bonusObjectivesContainer, false);
                MissionSelectorObjective handler = obj.GetComponent<MissionSelectorObjective>();
                handler.Initialize(objective, isBonus: true);
            }
        }

    }
    void InitializeIntel(LevelTemplate template) {
        descriptionText.text = template.shortDescription;
        guardSummaryText.text = template.guardAlertness switch {
            Alertness.distracted => "Security posture: Relaxed",
            Alertness.normal => "Security posture: Normal",
            Alertness.alert => "Security posture: Alert",
        };
        guardDescText.text = template.guardAlertness switch {
            Alertness.distracted => "Guards are not expecting to encounter trouble today.",
            Alertness.normal => "Guards are on the lookout for ordinary security threats.",
            Alertness.alert => "Guards are on alert as they patrol the premises.",
        };
        physicalSummaryText.text = template.securityLevel switch {
            LevelTemplate.SecurityLevel.lax => "Status: Unsecure",
            LevelTemplate.SecurityLevel.commercial => "Status: Commercial grade",
            LevelTemplate.SecurityLevel.hardened => "Status: Hardened",
        };
        physicalDescText.text = template.securityDescription;
    }
}
