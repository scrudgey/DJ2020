using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MissionComputerController : MonoBehaviour {
    public AudioSource audioSource;
    public AudioClip[] openMenuSound;
    public AudioClip[] missionButtonClickSound;
    public Transform missionButtonContainer;
    public TextMeshProUGUI rewardAmountText;
    public TextMeshProUGUI creditAmountText;
    public Image factionLogo;
    public TextMeshProUGUI factionText;
    public TextMeshProUGUI emailText;
    public Transform objectivesContainer;

    public GameObject missionButtonPrefab;
    public GameObject objectivePrefab;
    public GameObject spacerPrefab;
    public GameObject dividerPrefab;

    public LevelTemplate activeLevelTemplate;
    GameData data;

    public void Initialize(GameData data) {
        Toolbox.RandomizeOneShot(audioSource, openMenuSound);
        this.data = data;
        LevelTemplate defaultTemplate = null;
        foreach (Transform child in missionButtonContainer) {
            Destroy(child.gameObject);
        }

        foreach (string levelName in data.unlockedLevels) {
            LevelTemplate template = LevelTemplate.LoadAsInstance(levelName);
            CreateMissionButton(template);
            if (defaultTemplate == null) {
                defaultTemplate = template;
            }
        }
        if (defaultTemplate != null) {
            LoadTemplate(defaultTemplate);
        }
    }
    void CreateMissionButton(LevelTemplate template) {
        GameObject obj = GameObject.Instantiate(missionButtonPrefab);
        obj.transform.SetParent(missionButtonContainer, false);
        MissionSelectorButton selectorButton = obj.GetComponent<MissionSelectorButton>();
        selectorButton.Initialize(this, template);
    }
    public void MissionButtonCallback(MissionSelectorButton button) {
        Toolbox.RandomizeOneShot(audioSource, missionButtonClickSound);
        LoadTemplate(button.template);
    }

    public void LoadTemplate(LevelTemplate template) {
        activeLevelTemplate = template;
        Debug.Log($"Load {template.name}");

        rewardAmountText.text = template.creditReward.ToString("#,#");
        creditAmountText.text = data.playerState.credits.ToString("#,#");

        factionLogo.sprite = template.faction.logo;
        factionText.text = template.faction.description;

        emailText.text = template.prompt.text;

        foreach (Transform child in objectivesContainer) {
            if (child.name == "title") continue;
            Destroy(child.gameObject);
        }
        foreach (Objective objective in template.objectives) {
            GameObject obj = GameObject.Instantiate(objectivePrefab);
            obj.transform.SetParent(objectivesContainer, false);
            MissionSelectorObjective handler = obj.GetComponent<MissionSelectorObjective>();
            handler.Initialize(objective);
            // TextMeshProUGUI objText = obj.GetComponentInChildren<TextMeshProUGUI>();
            // objText.text = objective.title;
        }
        AddSpacer();
        AddDivider();
        AddSpacer();
    }
    public void AddSpacer() {
        GameObject spacerObj = GameObject.Instantiate(spacerPrefab);
        spacerObj.transform.SetParent(objectivesContainer, false);
    }
    public void AddDivider() {
        GameObject dividerObj = GameObject.Instantiate(dividerPrefab);
        dividerObj.transform.SetParent(objectivesContainer, false);
    }

    public void CancelButtonCallback() {
        Debug.Log("cancel");
        GameManager.I.HideMissionSelectMenu();
    }
    public void PlanButtonCallback() {
        Debug.Log($"Plan {activeLevelTemplate.name}");
        GameManager.I.ShowMissionPlanner(activeLevelTemplate);
    }
}
