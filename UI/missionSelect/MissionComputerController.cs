using System;
using System.Collections;
using System.Collections.Generic;
using Easings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class MissionComputerController : MonoBehaviour {
    public AudioSource audioSource;
    public GameObject UIEditorCamera;
    public Canvas myCanvas;


    [Header("missionSelect")]
    public GameObject contractsNotificationObject;
    public TextMeshProUGUI contractsNotificationText;
    public GameObject missionSelectBody;
    public Transform missionButtonContainer;
    public GameObject missionButtonPrefab;
    public GameObject objectivePrefab;
    public GameObject spacerPrefab;
    public GameObject dividerPrefab;


    [Header("details")]
    public GameObject detailsPanel;
    public RectTransform detailsRect;
    public TextMeshProUGUI rewardAmountText;
    public TextMeshProUGUI creditAmountText;
    public TextMeshProUGUI factionText;
    public TextMeshProUGUI emailText;
    public TextMeshProUGUI missionNameText;
    public TextMeshProUGUI taglineText;
    public Transform objectivesContainer;
    public GameObject bonusObjectiveDivider;

    [Header("faction")]
    public GameObject factionPanel;
    public RectTransform factionRect;
    public Image factionLogo;

    [Header("map")]
    public WorldmapView worldmapView;
    public GameObject mapPanel;
    public RectTransform mapRect;

    [Header("objects")]
    public GameObject rewardBox;
    [Header("sounds")]
    public AudioClip[] openMenuSound;
    public AudioClip[] missionButtonClickSound;
    [Header("softwareview")]
    public GameObject softwareViewObject;
    public MissionSelectSoftwareView missionSelectSoftwareView;
    [Header("softwareCraft")]
    public GameObject softwareCraftViewObject;
    public MissionSelectSoftwareCraftController softwareCraftController;
    [Header("modal")]
    public GameObject modalDialogue;
    public TextMeshProUGUI modalText;
    public GameObject modalAcceptButton;
    Action modalAcceptCallback;

    bool bottomIsShowing;
    bool factionIsShowing;
    LevelTemplate activeLevelTemplate;
    GameData data;
    List<MissionSelectorButton> selectorButtons;
    void Awake() {
        myCanvas.enabled = false;
        DestroyImmediate(UIEditorCamera);
    }

    public void Initialize(GameData data) {
        this.data = data;
        bottomIsShowing = false;
        factionIsShowing = false;
        mapPanel.SetActive(false);
        missionSelectBody.SetActive(false);
        factionPanel.SetActive(false);
        detailsPanel.SetActive(false);

        softwareViewObject.SetActive(false);
        softwareCraftViewObject.SetActive(false);
        softwareCraftController.CloseAddPayloadDialogue();
        modalDialogue.SetActive(false);

        Toolbox.RandomizeOneShot(audioSource, openMenuSound);
        selectorButtons = new List<MissionSelectorButton>();
        this.data = data;
        foreach (Transform child in missionButtonContainer) {
            Destroy(child.gameObject);
        }

        foreach (string levelName in data.unlockedLevels) {
            if (data.completedLevels.Contains(levelName)) continue;
            LevelTemplate template = LevelTemplate.LoadResource(levelName);
            MissionSelectorButton button = CreateMissionButton(template);
            selectorButtons.Add(button);
        }
        contractsNotificationObject.SetActive(data.unlockedLevels.Count > 0);
        contractsNotificationText.text = $"{data.unlockedLevels.Count}";
        ClearAll();

        myCanvas.enabled = true;
    }
    void ClearAll() {
        rewardAmountText.text = "-";
        creditAmountText.text = data.playerState.credits.ToString("#,#");

        factionLogo.enabled = false;
        factionText.text = "";

        emailText.text = "<NONE>";

        missionNameText.gameObject.SetActive(false);
        taglineText.gameObject.SetActive(false);
        rewardBox.SetActive(false);
    }
    MissionSelectorButton CreateMissionButton(LevelTemplate template) {
        GameObject obj = GameObject.Instantiate(missionButtonPrefab);
        obj.transform.SetParent(missionButtonContainer, false);
        MissionSelectorButton selectorButton = obj.GetComponent<MissionSelectorButton>();
        selectorButton.Initialize(this, template);
        return selectorButton;
    }
    public void MissionButtonCallback(MissionSelectorButton button) {
        if (!bottomIsShowing) {
            ShowDetailsDialogue();
        }
        Toolbox.RandomizeOneShot(audioSource, missionButtonClickSound);
        LoadTemplate(button.template);
        GUI.FocusControl(null);
    }
    public void MissionButtonMouseover(MissionSelectorButton button) {

        if (!factionIsShowing) {
            ShowMapAndFaction();
        }

        // set texts
        factionLogo.enabled = true;
        factionLogo.sprite = button.template.faction.logo;
        factionText.text = button.template.faction.factionName;
        worldmapView.HighlightPoint(button.template.worldmapPoint);
    }
    void ShowDetailsDialogue() {
        bottomIsShowing = true;
        detailsPanel.SetActive(true);
        EaseInDetailsPanel();
        foreach (MissionSelectorButton selectorButton in selectorButtons) {
            selectorButton.button.interactable = false;
        }
    }
    void HideDetailsDialogue() {
        bottomIsShowing = false;
        detailsPanel.SetActive(false);
        foreach (MissionSelectorButton selectorButton in selectorButtons) {
            selectorButton.button.interactable = true;
        }
    }

    void ShowMapAndFaction() {
        factionIsShowing = true;
        mapRect.sizeDelta = new Vector2(959, 35f);
        factionRect.sizeDelta = new Vector2(330f, 35f);
        mapPanel.SetActive(true);
        worldmapView.ShowText();
        StartCoroutine(Toolbox.Ease(null, 0.5f, 35f, 525f, PennerDoubleAnimation.ExpoEaseOut, (float height) => {
            mapRect.sizeDelta = new Vector2(959, height);
        }, unscaledTime: true));
        StartCoroutine(Toolbox.ChainCoroutines(
            new WaitForSecondsRealtime(0.2f),
            Toolbox.CoroutineFunc(() => factionPanel.SetActive(true)),
            Toolbox.Ease(null, 0.5f, 35f, 275f, PennerDoubleAnimation.ExpoEaseOut, (float height) => {
                factionRect.sizeDelta = new Vector2(330f, height);
            }, unscaledTime: true))
        );
    }
    void EaseInDetailsPanel() {
        StartCoroutine(Toolbox.Ease(null, 0.2f, 50, 1700, PennerDoubleAnimation.ExpoEaseOut, (amount) => {
            detailsRect.sizeDelta = new Vector2(amount, detailsRect.rect.height);
        }, unscaledTime: true));
        StartCoroutine(Toolbox.Ease(null, 0.1f, 50, 800, PennerDoubleAnimation.ExpoEaseOut, (amount) => {
            detailsRect.sizeDelta = new Vector2(detailsRect.rect.width, amount);
        }, unscaledTime: true));
    }

    public void LoadTemplate(LevelTemplate template) {
        activeLevelTemplate = template;

        missionNameText.gameObject.SetActive(true);
        taglineText.gameObject.SetActive(true);
        rewardBox.SetActive(true);

        rewardAmountText.text = template.creditReward.ToString("#,#");
        creditAmountText.text = data.playerState.credits.ToString("#,#");

        emailText.text = template.proposalEmail.text;

        missionNameText.text = template.readableMissionName;
        taglineText.text = template.tagline;

        InitializeObjectives(template);
    }
    void InitializeObjectives(LevelTemplate template) {
        bonusObjectiveDivider.SetActive(false);

        foreach (Transform child in objectivesContainer) {
            if (child.name == "title" || child.gameObject == bonusObjectiveDivider) continue;
            Destroy(child.gameObject);
        }
        foreach (Objective objective in template.objectives) {
            MissionSelectorObjective handler = spawnObjective();
            handler.Initialize(objective);
            // AddSpacer();
        }
        if (template.bonusObjectives.Count > 0) {
            // AddDivider();
            bonusObjectiveDivider.SetActive(true);
            bonusObjectiveDivider.transform.SetAsLastSibling();
            foreach (Objective objective in template.bonusObjectives) {
                MissionSelectorObjective handler = spawnObjective();
                handler.Initialize(objective, isBonus: true);
                // AddSpacer();
            }
        }
    }
    MissionSelectorObjective spawnObjective() {
        GameObject obj = GameObject.Instantiate(objectivePrefab);
        obj.transform.SetParent(objectivesContainer, false);
        return obj.GetComponent<MissionSelectorObjective>();
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
        GameManager.I.HideMissionSelectMenu();
    }
    public void PlanButtonCallback() {
        GameManager.I.CloseMenu();
        GameManager.I.ShowMissionPlanner(activeLevelTemplate);
    }
    public void ContractsButtonCallback() {
        missionSelectBody.SetActive(true);
        softwareViewObject.SetActive(false);
        softwareCraftViewObject.SetActive(false);
        modalDialogue.SetActive(false);
    }
    public void SoftwareButtonCallback() {
        ShowSoftwareListView(true);
    }
    public void ShowSoftwareListView(bool value) {
        if (value) {
            missionSelectSoftwareView.Initialize(data);
        }
        missionSelectBody.SetActive(false);
        softwareViewObject.SetActive(value);
        softwareCraftViewObject.SetActive(false);
        modalDialogue.SetActive(false);
    }
    public void ShowSoftwareCraftView(bool value) {
        softwareCraftViewObject.SetActive(value);
        softwareCraftController.Initialize(data);
    }
    public void ShowSoftwareCraftViewEditMode(SoftwareTemplate template) {
        softwareCraftViewObject.SetActive(true);
        softwareCraftController.Initialize(data, template);
    }

    public void ShowModalDialogue(string text, Action acceptAction) {
        modalAcceptCallback = acceptAction;
        modalText.text = text;
        modalDialogue.SetActive(true);
        modalAcceptButton.SetActive(true);
    }
    public void ShowModalDialogue(string text) {
        modalAcceptButton.SetActive(false);
        modalText.text = text;
        modalDialogue.SetActive(true);
    }
    public void ModalAcceptCallback() {
        modalDialogue.SetActive(false);
        modalAcceptCallback?.Invoke();
    }
    public void ModalCancelCallback() {
        modalDialogue.SetActive(false);
    }

    public void DetailPanelCloseCallback() {
        HideDetailsDialogue();
    }
}
