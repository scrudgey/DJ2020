using System.Collections;
using System.Collections.Generic;
using Easings;
using Easings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class MissionComputerController : MonoBehaviour {
    public AudioSource audioSource;
    public AudioClip[] openMenuSound;
    public AudioClip[] missionButtonClickSound;
    public Transform missionButtonContainer;
    public GameObject missionButtonPrefab;
    public GameObject objectivePrefab;
    public GameObject spacerPrefab;
    public GameObject dividerPrefab;

    public LevelTemplate activeLevelTemplate;
    GameData data;
    List<MissionSelectorButton> selectorButtons;

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

    [Header("faction")]
    bool factionIsShowing;
    public GameObject factionPanel;
    public RectTransform factionRect;
    public Image factionLogo;

    [Header("map")]
    public GameObject mapPanel;
    public RectTransform mapRect;
    public Image mapImage;
    public Color mapColor1;
    public Color mapColor2;
    public float timer;
    public float colorFlipInterval;
    public TextMeshProUGUI mapDescriptionTitle;
    public TextMeshProUGUI mapDescriptionBody;
    public TextMeshProUGUI mapDescriptionTarget;

    [Header("objects")]
    public GameObject rewardBox;
    [Header("easing")]
    public LayoutElement bottomLayoutElement;
    bool bottomIsShowing;
    List<Coroutine> blitRoutines;

    public void Initialize(GameData data) {
        // bottomPanel.SetActive(false);
        blitRoutines = new List<Coroutine>();
        bottomIsShowing = false;
        factionIsShowing = false;
        mapPanel.SetActive(false);
        factionPanel.SetActive(false);
        detailsPanel.SetActive(false);
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
        ClearAll();
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
        // factionBox.SetActive(false);
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
        // foreach (MissionSelectorButton selectorButton in selectorButtons) {
        //     selectorButton.SetFocus(selectorButton == button);
        // }
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

        foreach (Coroutine coroutine in blitRoutines) {
            StopCoroutine(coroutine);
        }

        blitRoutines.Clear();
        blitRoutines.Add(StartCoroutine(Toolbox.BlitText(mapDescriptionTitle, "NEO BOSTON")));
        blitRoutines.Add(StartCoroutine(Toolbox.BlitText(mapDescriptionBody, "Pop: 2,654,776\n42°21′37″N\n71°3′28″W")));
        blitRoutines.Add(StartCoroutine(Toolbox.BlitText(mapDescriptionTarget, $"TARGET: {button.template.sceneDescriptor}")));
    }
    void ShowDetailsDialogue() {
        bottomIsShowing = true;
        detailsPanel.SetActive(true);
        StartCoroutine(EaseInDetailsPanel());
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
        mapRect.sizeDelta = new Vector2(530f, 35f);
        factionRect.sizeDelta = new Vector2(330f, 35f);
        mapPanel.SetActive(true);
        StartCoroutine(Toolbox.Ease(null, 0.5f, 35f, 550f, PennerDoubleAnimation.ExpoEaseOut, (float height) => {
            mapRect.sizeDelta = new Vector2(530f, height);
        }, unscaledTime: true));
        StartCoroutine(Toolbox.ChainCoroutines(
            new WaitForSecondsRealtime(0.2f),
            Toolbox.CoroutineFunc(() => factionPanel.SetActive(true)),
            Toolbox.Ease(null, 0.5f, 35f, 275f, PennerDoubleAnimation.ExpoEaseOut, (float height) => {
                factionRect.sizeDelta = new Vector2(330f, height);
            }, unscaledTime: true))
        );
    }
    IEnumerator EaseInPanel() {
        float timer = 0f;
        float duration = 1f;
        while (timer < duration) {
            timer += Time.unscaledDeltaTime;
            float size = (float)PennerDoubleAnimation.ExpoEaseOut(timer, 200, 425, duration);
            bottomLayoutElement.minHeight = size;
            yield return null;
        }
        bottomLayoutElement.minHeight = 625f;
    }
    IEnumerator EaseInDetailsPanel() {
        float timer = 0f;
        float duration = 0.4f;
        detailsRect.sizeDelta = new Vector2(50f, 50f);
        while (timer < duration) {
            timer += Time.unscaledDeltaTime;
            float width = (float)PennerDoubleAnimation.ExpoEaseOut(timer, 50, 1750, duration);
            detailsRect.sizeDelta = new Vector2(width, 50f);
            yield return null;
        }
        timer = 0f;
        while (timer < duration) {
            timer += Time.unscaledDeltaTime;
            float height = (float)PennerDoubleAnimation.ExpoEaseOut(timer, 50, 940, duration);
            detailsRect.sizeDelta = new Vector2(1800f, height);
            yield return null;
        }
        detailsRect.sizeDelta = new Vector2(1800f, 990f);
    }

    public void LoadTemplate(LevelTemplate template) {
        activeLevelTemplate = template;

        missionNameText.gameObject.SetActive(true);
        taglineText.gameObject.SetActive(true);
        rewardBox.SetActive(true);
        // factionBox.SetActive(true);

        rewardAmountText.text = template.creditReward.ToString("#,#");
        creditAmountText.text = data.playerState.credits.ToString("#,#");

        // factionLogo.enabled = true;
        // factionLogo.sprite = template.faction.logo;
        // factionText.text = template.faction.description;

        emailText.text = template.proposalEmail.text;

        missionNameText.text = template.levelName;
        taglineText.text = template.tagline;

        InitializeObjectives(template);
    }
    void InitializeObjectives(LevelTemplate template) {
        foreach (Transform child in objectivesContainer) {
            if (child.name == "title") continue;
            Destroy(child.gameObject);
        }
        foreach (Objective objective in template.objectives) {
            GameObject obj = GameObject.Instantiate(objectivePrefab);
            obj.transform.SetParent(objectivesContainer, false);
            MissionSelectorObjective handler = obj.GetComponent<MissionSelectorObjective>();
            handler.Initialize(objective);
            AddSpacer();
            // TextMeshProUGUI objText = obj.GetComponentInChildren<TextMeshProUGUI>();
            // objText.text = objective.title;
        }
        // AddSpacer();
        // AddDivider();
        // AddSpacer();
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
        GameManager.I.ShowMissionPlanner(activeLevelTemplate);
    }

    public void DetailPanelCloseCallback() {
        HideDetailsDialogue();
    }

    void Update() {
        timer += Time.unscaledDeltaTime;
        if (timer > colorFlipInterval) {
            timer -= colorFlipInterval;
            if (mapImage.color == mapColor1) {
                mapImage.color = mapColor2;
            } else {
                mapImage.color = mapColor1;
            }
        }
    }
}
