using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PerkMenuController : MonoBehaviour {
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
    PlayerState state;
    PerkButton selectedPerkButton;
    void Awake() {
        DestroyImmediate(UIEditorCamera);
    }
    public void Initialize(PlayerState state) {
        this.state = state;
        ChangePane(PerkCategory.gun);
        RefreshDisplay(state);
    }

    void RefreshDisplay(PlayerState state) {
        playerHealth.text = $"HP: {(int)state.health} / {state.fullHealthAmount}";
        skillPointIndicator.text = $"{state.skillpoints}";

        bodyPoints.text = $"{state.bodySkillPoints}";
        gunPoints.text = $"{state.gunSkillPoints}";
        hackPoints.text = $"{state.hackSkillPoints}";
        speechPoints.text = $"{state.speechSkillPoints}";

        foreach (PerkButton button in GameObject.FindObjectsOfType(typeof(PerkButton), true)) {
            button.Initialize(this, state);
        }
    }
    public void CloseButtonCallback() {
        GameManager.I.HidePerkMenu();
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
    }

    public void PerkButtonCallback(PerkButton button) {
        selectedPerkButton = button;
        Debug.Log(button.perk.readableName);
        PopulatePerkView(button.perk);
    }
    public void ActivateButtonCallback() {
        state.ActivatePerk(selectedPerkButton.perk);
        RefreshDisplay(state);
        PerkButtonCallback(selectedPerkButton);
    }

    void PopulatePerkView(Perk perk) {
        perkViewTitle.text = perk.readableName;
        perkViewIcon.sprite = perk.icon;
        perkViewDescription.text = perk.description;
        string requirementText = "";
        if (perk.playerLevelRequirement > 0) {
            if (perk.PlayerLevelRequirementMet(state)) {
                requirementText += $"player level {perk.playerLevelRequirement}\n";
            } else {
                requirementText += $"<color=#ff4757>player level {perk.playerLevelRequirement}</color>\n";
            }
        }
        if (perk.skillLevelRequirement > 0) {
            if (perk.SkillLevelRequirementMet(state)) {
                requirementText += $"{perk.category} level {perk.skillLevelRequirement}\n";
            } else {
                requirementText += $"<color=#ff4757>{perk.category} level {perk.skillLevelRequirement}</color>\n";
            }
        }
        foreach (Perk requiredPerk in perk.requiredPerks) {
            if (perk.PerkRequirementMet(state, requiredPerk)) {
                requirementText += $"{requiredPerk.readableName}\n";
            } else {
                requirementText += $"<color=#ff4757>{requiredPerk.readableName}</color>\n";
            }
        }
        perkViewRequirements.text = requirementText;

        if (state.PerkIsFullyActivated(perk)) {
            perkViewActiveText.gameObject.SetActive(true);
            perkViewActivateButton.gameObject.SetActive(false);
            perkViewActiveText.text = "activated";

            perkViewActivateButton.interactable = false;
        } else if (perk.CanBePurchased(state)) {
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
