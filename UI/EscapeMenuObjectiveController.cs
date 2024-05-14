using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Easings;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
public class EscapeMenuObjectiveController : MonoBehaviour {
    [Header("objective")]
    public Transform objectivesContainer;
    public GameObject objectiveIndicatorPrefab;
    public GameObject bonusObjectiveHeader;
    public TextMeshProUGUI missionTitle;
    public TextMeshProUGUI missionTagline;
    public TextMeshProUGUI missionStatusText;

    public void Initialize(LevelState levelState) {
        foreach (Transform child in objectivesContainer) {
            if (child.gameObject == bonusObjectiveHeader) continue;
            if (child.name.ToLower().Contains("spacer")) continue;
            Destroy(child.gameObject);
        }
        foreach (ObjectiveDelta objective in levelState.delta.objectiveDeltas) {
            GameObject obj = GameObject.Instantiate(objectiveIndicatorPrefab);
            obj.transform.SetParent(objectivesContainer, false);
            MissionSelectorObjective controller = obj.GetComponent<MissionSelectorObjective>();
            controller.Initialize(objective);
        }
        if (levelState.delta.optionalObjectiveDeltas.Count > 0) {
            bonusObjectiveHeader.gameObject.SetActive(true);
            bonusObjectiveHeader.transform.SetAsLastSibling();
            foreach (ObjectiveDelta objective in levelState.delta.optionalObjectiveDeltas) {
                GameObject obj = GameObject.Instantiate(objectiveIndicatorPrefab);
                obj.transform.SetParent(objectivesContainer, false);
                MissionSelectorObjective controller = obj.GetComponent<MissionSelectorObjective>();
                controller.Initialize(objective, isBonus: true);
            }
        } else {
            bonusObjectiveHeader.gameObject.SetActive(false);
        }
        missionTitle.text = levelState.template.readableMissionName;
        missionTagline.text = levelState.template.tagline;
        if (levelState.delta.phase == LevelDelta.MissionPhase.extractionFail) {
            missionStatusText.text = "status: failed";
        } else {
            missionStatusText.text = "status: active";
        }
    }
}
