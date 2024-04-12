using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Easings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class PerkMenuController : MonoBehaviour {
    public Canvas myCanvas;
    public AudioSource audioSource;
    public PerkCategory currentPane;
    public GameObject UIEditorCamera;
    [Header("color")]
    public Color bodyColor;
    public Color gunColor;
    public Color hackColor;
    public Color speechColor;
    public Image outlineImage;
    public Image headerOutlineImage;
    public Image headerBackgroundImage;
    [Header("categories")]
    public Image bodyHighlight;
    public Image gunHighlight;
    public Image hackHighlight;
    public Image speechHighlight;
    public GameObject bodyButtons;
    public GameObject gunButtons;
    public GameObject hackButtons;
    public GameObject speechButtons;
    [Header("display")]
    public TextMeshProUGUI categoryTitle;
    public TextMeshProUGUI skillPointIndicator;
    public TextMeshProUGUI playerName;
    public TextMeshProUGUI playerLevel;
    public TextMeshProUGUI playerHealth;


    public TextMeshProUGUI bodyPoints;
    public TextMeshProUGUI gunPoints;
    public TextMeshProUGUI hackPoints;
    public TextMeshProUGUI speechPoints;
    [Header("perkview")]
    public TextMeshProUGUI perkViewTitle;
    public Image perkViewIcon;
    public TextMeshProUGUI perkViewDescription;
    public TextMeshProUGUI perkViewRequirements;
    public Button perkViewActivateButton;
    public TextMeshProUGUI perkViewActiveText;
    public TextMeshProUGUI requirementTitleText;
    public GameObject perkviewContainer;
    public RectTransform perkviewRect;
    // public List<PerkMenuBoxController> perkMenuBoxes;
    [Header("sfx")]
    public AudioClip[] selectPerkSound;
    public AudioClip[] activatePerkSound;
    public AudioClip[] disclosePerkSound;
    public AudioClip[] changePaneSound;
    public AudioClip[] showSounds;
    public AudioClip[] closeSounds;

    bool perkViewActive;
    PlayerState state;
    PerkButton selectedPerkButton;
    void Awake() {
        myCanvas.enabled = false;
        DestroyImmediate(UIEditorCamera);
    }
    public void Initialize(GameData data, PlayerState state) {
        this.state = state;
        GameManager.I.PlayUISound(showSounds);
        playerName.text = data.filename;
        ChangePane(PerkCategory.gun);
        RefreshDisplay(state);
        HidePerkView();
        // EaseInButtons();
        myCanvas.enabled = true;
    }
    void HidePerkView() {
        perkViewActive = false;
        perkviewContainer.SetActive(false);
        perkviewRect.sizeDelta = new Vector2(450, 50f);
    }
    void EaseInButtons() {
        float delay = 0f;
        foreach (PerkButton button in GameObject.FindObjectsOfType(typeof(PerkButton), true).OrderBy(perkbutton => ((PerkButton)perkbutton).transform.position.x)) {
            // button.Initialize(this, state);
            // button.SetSelected(false);
            button.EaseIn(delay);
            delay += 0.005f;
        }
    }
    void RefreshDisplay(PlayerState state) {
        playerHealth.text = $"HP: {(int)state.health} / {state.fullHealthAmount()}";
        playerLevel.text = $"Level: {state.PlayerLevel()}";
        skillPointIndicator.text = $"Skill points: {state.skillpoints}";

        bodyPoints.text = $"{state.bodySkillPoints}";
        gunPoints.text = $"{state.gunSkillPoints}";
        hackPoints.text = $"{state.hackSkillPoints}";
        speechPoints.text = $"{state.speechSkillPoints}";

        foreach (PerkButton button in GameObject.FindObjectsOfType(typeof(PerkButton), true)) {
            button.Initialize(this, state);
            button.SetSelected(false);
        }

        if (selectedPerkButton != null) {
            selectedPerkButton.SetSelected(true);
        }
    }
    public void CloseButtonCallback() {
        GameManager.I.HidePerkMenu();
        GameManager.I.PlayUISound(closeSounds);
    }

    public void NextPaneCallback() {
        int index = (int)currentPane + 1;
        if (index > 3) index = 0;
        PerkCategory newPane = (PerkCategory)index;
        ChangePane(newPane);
    }
    public void PreviousPaneCallback() {
        int index = (int)currentPane - 1;
        if (index < 0) index = 3;
        PerkCategory newPane = (PerkCategory)index;
        ChangePane(newPane);
    }
    public void SelectPaneCallback(PerkCategory category) {
        ChangePane(category);
    }

    void ChangePane(PerkCategory newPane) {
        Toolbox.RandomizeOneShot(audioSource, changePaneSound);

        currentPane = newPane;

        categoryTitle.text = $"{newPane}";

        bodyButtons.SetActive(false);
        gunButtons.SetActive(false);
        hackButtons.SetActive(false);
        speechButtons.SetActive(false);

        bodyHighlight.enabled = false;
        gunHighlight.enabled = false;
        hackHighlight.enabled = false;
        speechHighlight.enabled = false;


        (GameObject pane, Image highlight, Color c) = newPane switch {
            PerkCategory.body => (bodyButtons, bodyHighlight, bodyColor),
            PerkCategory.gun => (gunButtons, gunHighlight, gunColor),
            PerkCategory.hack => (hackButtons, hackHighlight, hackColor),
            PerkCategory.speech => (speechButtons, speechHighlight, speechColor)
        };

        pane.SetActive(true);
        highlight.enabled = true;

        outlineImage.color = c;
        headerOutlineImage.color = c;
        headerBackgroundImage.color = c;

        EaseInButtons();
    }

    public void PerkButtonCallback(PerkButton button) {
        if (selectedPerkButton != null) {
            selectedPerkButton.SetSelected(false);
        }
        selectedPerkButton = button;
        selectedPerkButton.SetSelected(true);
        PopulatePerkView(button.perk);
        if (!perkViewActive) {
            Toolbox.RandomizeOneShot(audioSource, disclosePerkSound);
            ShowPerkView();
        } else {
            Toolbox.RandomizeOneShot(audioSource, selectPerkSound);
        }
    }
    void ShowPerkView() {
        perkViewActive = true;
        perkviewContainer.SetActive(true);
        Toolbox.RandomizeOneShot(audioSource, showSounds);
        StartCoroutine(Toolbox.Ease(null, 0.15f, 50f, 740f, PennerDoubleAnimation.Linear, (float amount) => {
            perkviewRect.sizeDelta = new Vector2(450f, amount);
        }, unscaledTime: true));
    }
    public void ActivateButtonCallback() {
        state.ActivatePerk(selectedPerkButton.perk);
        RefreshDisplay(state);
        PerkButtonCallback(selectedPerkButton);
        Toolbox.RandomizeOneShot(audioSource, activatePerkSound);
    }

    void PopulatePerkView(Perk perk) {
        perkViewTitle.text = perk.readableName;
        perkViewIcon.sprite = perk.icon;
        perkViewDescription.text = perk.description;
        string requirementText = "";
        bool requirementSet = false;
        if (perk.playerLevelRequirement > 0) {
            if (perk.PlayerLevelRequirementMet(state)) {
                requirementText += $"player level {perk.playerLevelRequirement}\n";
            } else {
                requirementText += $"<color=#ff4757>player level {perk.playerLevelRequirement}</color>\n";
            }
            requirementSet = true;
        }
        if (perk.skillLevelRequirement > 0) {
            if (perk.SkillLevelRequirementMet(state)) {
                requirementText += $"{perk.category} level {perk.skillLevelRequirement}\n";
            } else {
                requirementText += $"<color=#ff4757>{perk.category} level {perk.skillLevelRequirement}</color>\n";
            }
            requirementSet = true;
        }
        foreach (Perk requiredPerk in perk.requiredPerks) {
            if (perk.PerkRequirementMet(state, requiredPerk)) {
                requirementText += $"{requiredPerk.readableName}\n";
            } else {
                requirementText += $"<color=#ff4757>{requiredPerk.readableName}</color>\n";
            }
            requirementSet = true;
        }
        requirementTitleText.enabled = requirementSet;
        perkViewRequirements.text = requirementText;

        if (state.PerkIsFullyActivated(perk)) {
            perkViewActiveText.gameObject.SetActive(true);
            perkViewActivateButton.gameObject.SetActive(false);
            perkViewActiveText.text = "activated";

            perkViewActivateButton.interactable = false;
        } else if (state.skillpoints > 0 && perk.CanBePurchased(state)) {
            perkViewActiveText.gameObject.SetActive(false);
            perkViewActivateButton.gameObject.SetActive(true);

            perkViewActivateButton.interactable = true;
        } else {
            perkViewActiveText.gameObject.SetActive(true);
            perkViewActivateButton.gameObject.SetActive(false);
            perkViewActiveText.text = "<color=#ff4757>locked</color>";

            perkViewActivateButton.interactable = false;
        }
    }
}
