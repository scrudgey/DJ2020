using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Easings;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
public class EscapeMenuController : MonoBehaviour {

    public Canvas myCanvas;

    public GameObject UIEditorCamera;
    public RectTransform menuRect;

    [Header("tabs")]
    public GameObject objectiveTab;
    public Button objectiveTabButton;

    public GameObject mapTab;
    public Button mapTabButton;

    public GameObject lootTab;
    public Button lootTabButton;
    public TextMeshProUGUI lootCount;

    public GameObject keyTab;
    public Button keyTabButton;
    public TextMeshProUGUI keyCount;

    public GameObject dataTab;
    public Button dataTabButton;
    public TextMeshProUGUI dataCount;

    public GameObject characterTab;
    public Button characterTabButton;
    [Header("modal")]
    public GameObject modalDialogObject;
    public CanvasGroup nonModalCanvasGroup;
    public Action modalAcceptCallback;

    [Header("objective")]
    public EscapeMenuObjectiveController objectiveController;
    [Header("map")]
    public MapDisplay3DView mapDisplayView;
    [Header("loot")]
    public EscapeMenuLootController lootController;
    [Header("key")]
    public EscapeMenuKeyController keyController;
    [Header("data")]
    public EscapeMenuDataController dataController;
    [Header("character")]
    public EscapeMenuCharacterTabController characterController;

    [Header("colors")]
    public ColorBlock normalTabColors;
    public ColorBlock selectedTabColors;
    [Header("sounds")]
    public AudioClip[] openSounds;
    public AudioClip[] closeSounds;
    Coroutine menuSizeCoroutine;

    void Awake() {
        myCanvas.enabled = false;
        modalDialogObject.SetActive(false);
        DestroyImmediate(UIEditorCamera);
    }
    public void Initialize(GameData data, string sceneName) {
        GameManager.I.PlayUISound(openSounds);
        SceneData sceneData = SceneData.loadSceneData(sceneName);

        int currentLoots = data.playerState.loots.Count;
        int currentData = data.playerState.payDatas.Count;
        int currentKeys = 0;
        characterController.Initialize(data);

        if (data.phase == GamePhase.world) {
            objectiveTabButton.gameObject.SetActive(false);
            keyTabButton.gameObject.SetActive(false);

            mapDisplayView.InitializeWorldMode(sceneData);

            lootController.Initialize(data.playerState.loots.ToList());
            dataController.Initialize(data.playerState.payDatas.ToList());

            ChangeTabCallback("character");
        } else {
            currentLoots += data.levelState.delta.levelAcquiredLoot.Count;
            currentData += data.levelState.delta.levelAcquiredPaydata.Count;
            currentKeys = data.levelState.TotalNumberKeys();

            objectiveController.Initialize(data.levelState);
            mapDisplayView.Initialize(data.levelState, sceneData);
            lootController.Initialize(data.levelState.delta.levelAcquiredLoot.Concat(data.playerState.loots).ToList());
            dataController.Initialize(data.playerState.payDatas.Concat(data.levelState.delta.levelAcquiredPaydata).ToList());
            keyController.Initialize(data.levelState.delta);

            ChangeTabCallback("objective");
        }

        lootCount.text = $"{currentLoots}";
        dataCount.text = $"{currentData}";
        keyCount.text = $"{currentKeys}";

        myCanvas.enabled = true;
    }

    public void ContinueButtonCallback() {
        GameManager.I.CloseMenu();
        GameManager.I.PlayUISound(closeSounds);
    }
    public void AbortButtonCallback() {
        ShowModal(DoAbort);
    }

    void DoAbort() {
        if (GameManager.I.gameData.levelState.template.isTutorial) {
            CutsceneManager.I.StopAllCutscenes();
            Sprite jackPortrait = Resources.Load<Sprite>("sprites/portraits/Jack") as Sprite;
            CutsceneManager.I.StartCutscene(new AbortTutorialCutscene(jackPortrait));
            GameManager.I.CloseMenu();
        } else {
            GameManager.I.CloseMenu();
            GameManager.I.HandleObjectiveFailed();
        }
    }
    public void HandleEscapeAction(InputAction.CallbackContext ctx) {
        ContinueButtonCallback();
    }
    public void SkillMenuCallback() {
        GameManager.I.ShowPerkMenu();
    }

    public void ChangeTabCallback(string tabName) {
        objectiveTab.SetActive(false);
        mapTab.SetActive(false);
        lootTab.SetActive(false);
        keyTab.SetActive(false);
        dataTab.SetActive(false);
        characterTab.SetActive(false);
        switch (tabName) {
            case "objective":
                objectiveTab.SetActive(true);
                SetButtonSelectedColor(objectiveTabButton);
                ChangeMenuSize(1400);
                break;
            case "map":
                mapTab.SetActive(true);
                SetButtonSelectedColor(mapTabButton);
                ChangeMenuSize(1400);
                break;
            case "loot":
                lootTab.SetActive(true);
                SetButtonSelectedColor(lootTabButton);
                ChangeMenuSize(1060);
                break;
            case "key":
                keyTab.SetActive(true);
                SetButtonSelectedColor(keyTabButton);
                ChangeMenuSize(1060);
                break;
            case "data":
                dataTab.SetActive(true);
                SetButtonSelectedColor(dataTabButton);
                ChangeMenuSize(1060);
                break;
            case "character":
                characterTab.SetActive(true);
                SetButtonSelectedColor(characterTabButton);
                ChangeMenuSize(1060);
                break;
        }
    }

    void ChangeMenuSize(float width) {
        if (menuSizeCoroutine != null) {
            StopCoroutine(menuSizeCoroutine);
        }
        menuSizeCoroutine = StartCoroutine(ChangeSize(width));
    }

    IEnumerator ChangeSize(float width) {
        float height = menuRect.rect.height;
        return Toolbox.Ease(null, 0.15f, menuRect.rect.width, width, PennerDoubleAnimation.Linear, (amount) => {
            menuRect.sizeDelta = new Vector2(amount, height);
        }, unscaledTime: true);
    }

    void SetButtonSelectedColor(Button target) {
        objectiveTabButton.colors = normalTabColors;
        mapTabButton.colors = normalTabColors;
        dataTabButton.colors = normalTabColors;
        keyTabButton.colors = normalTabColors;
        lootTabButton.colors = normalTabColors;
        characterTabButton.colors = normalTabColors;

        target.colors = selectedTabColors;
    }

    public void ShowModal(Action callback) {
        modalAcceptCallback = callback;
        modalDialogObject.SetActive(true);
        nonModalCanvasGroup.interactable = false;
    }
    public void ModalAcceptButtonCallback() {
        modalDialogObject.SetActive(false);
        nonModalCanvasGroup.interactable = true;
        modalAcceptCallback?.Invoke();
    }
    public void ModalCancelButtonCallback() {
        modalDialogObject.SetActive(false);
        nonModalCanvasGroup.interactable = true;
    }
}
