using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class MedicalShopController : MonoBehaviour {

    public GameObject UIEditorCamera;
    public RectTransform bottomRect;
    public AudioSource audioSource;
    public StoreDialogueController storeDialogueController;
    [Header("stats")]
    public HealthIndicatorController healthIndicatorController;
    public TextMeshProUGUI playerCredits;
    public TextMeshProUGUI costToHeal;
    public TextMeshProUGUI playerStats;
    public GameObject costToHealObject;
    [Header("buttons")]
    public Button healButton;
    public Button legButton;
    public Button armButton;
    public Button eyeButton;

    [Header("sounds")]
    public AudioClip[] healSounds;
    public AudioClip[] discloseBottomSound;

    void Awake() {
        DestroyImmediate(UIEditorCamera);
        bottomRect.sizeDelta = new Vector2(1f, 0f);
    }
    public void Initialize() {

        storeDialogueController.Initialize(GameManager.I.gameData.filename, "Dr. Head");

        storeDialogueController.SetShopownerDialogue("My work is clean. No questions asked.");
        StartCoroutine(Toolbox.OpenStore(bottomRect, audioSource, discloseBottomSound));
        SetPlayerStats();
    }

    void SetPlayerStats() {
        playerCredits.text = $"{GameManager.I.gameData.playerState.credits}";
        // costToHeal.text = $""
        float health = GameManager.I.gameData.playerState.health;
        float fullHealth = GameManager.I.gameData.playerState.fullHealthAmount;
        costToHealObject.SetActive(health < fullHealth);
        healButton.interactable = health < fullHealth;
        costToHeal.text = $"{fullHealth - health}";

        playerStats.text = $"HP: {health}/{fullHealth}\nEyes: v{GameManager.I.gameData.playerState.cyberEyesThermal}\nArms: v0\nLegs: v{GameManager.I.gameData.playerState.cyberlegsLevel}";

        healthIndicatorController.SetHealthDisplay(health, fullHealth);
    }


    public void HealButtonCallback() {
        float health = GameManager.I.gameData.playerState.health;
        float fullHealth = GameManager.I.gameData.playerState.fullHealthAmount;
        float cost = fullHealth - health;
        if (GameManager.I.gameData.playerState.credits >= cost) {
            Toolbox.RandomizeOneShot(audioSource, healSounds);
            GameManager.I.gameData.playerState.credits -= (int)cost;
            GameManager.I.gameData.playerState.health = GameManager.I.gameData.playerState.fullHealthAmount;
            storeDialogueController.SetShopownerDialogue("Let's get you patched up.");
        } else {
            storeDialogueController.SetShopownerDialogue("Come back when you have the credits.");
        }

        SetPlayerStats();
    }

    public void LegButtonCallback() {

    }
    public void ArmButtonCallback() {

    }
    public void EyeButtonCallback() {

    }

    public void DoneButtonCallback() {
        GameManager.I.CloseMenu();
    }
}
