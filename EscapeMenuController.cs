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


    [Header("objective")]
    public Transform objectivesContainer;
    public GameObject objectiveIndicatorPrefab;
    public GameObject bonusObjectiveHeader;
    public TextMeshProUGUI missionTitle;
    public TextMeshProUGUI missionTagline;
    public TextMeshProUGUI missionStatusText;
    [Header("map")]
    public MapDisplay3DView mapDisplayView;
    [Header("loot")]
    public EscapeMenuLootController lootController;
    [Header("key")]
    public EscapeMenuKeyController keyController;
    [Header("data")]
    public EscapeMenuDataController dataController;
    [Header("colors")]
    public ColorBlock normalTabColors;
    public ColorBlock selectedTabColors;
    [Header("sounds")]
    public AudioClip[] openSounds;
    public AudioClip[] closeSounds;
    Coroutine menuSizeCoroutine;

    void Awake() {
        myCanvas.enabled = false;
        DestroyImmediate(UIEditorCamera);
    }
    public void Initialize(GameData data) {
        GameManager.I.PlayUISound(openSounds);
        foreach (Transform child in objectivesContainer) {
            if (child.gameObject == bonusObjectiveHeader) continue;
            if (child.name.ToLower().Contains("spacer")) continue;
            Destroy(child.gameObject);
        }
        foreach (ObjectiveDelta objective in data.levelState.delta.objectiveDeltas) {
            GameObject obj = GameObject.Instantiate(objectiveIndicatorPrefab);
            obj.transform.SetParent(objectivesContainer, false);
            MissionSelectorObjective controller = obj.GetComponent<MissionSelectorObjective>();
            controller.Initialize(objective);
        }
        if (data.levelState.delta.optionalObjectiveDeltas.Count > 0) {
            bonusObjectiveHeader.gameObject.SetActive(true);
            bonusObjectiveHeader.transform.SetAsLastSibling();
            foreach (ObjectiveDelta objective in data.levelState.delta.optionalObjectiveDeltas) {
                GameObject obj = GameObject.Instantiate(objectiveIndicatorPrefab);
                obj.transform.SetParent(objectivesContainer, false);
                MissionSelectorObjective controller = obj.GetComponent<MissionSelectorObjective>();
                controller.Initialize(objective, isBonus: true);
            }
        } else {
            bonusObjectiveHeader.gameObject.SetActive(false);
        }
        missionTitle.text = data.levelState.template.readableMissionName;
        missionTagline.text = data.levelState.template.tagline;
        missionStatusText.text = "status: active";
        mapDisplayView.Initialize(data.levelState);
        lootController.Initialize(data.levelState.delta.levelAcquiredLoot.Concat(data.playerState.loots).ToList());
        keyController.Initialize(data.levelState);
        dataController.Initialize(data);

        int currentLoots = data.playerState.loots.Count + data.levelState.delta.levelAcquiredLoot.Count;
        int currentData = data.playerState.payDatas.Count + data.levelState.delta.levelAcquiredPaydata.Count;
        int numberPasswords = data.playerState.payDatas.Concat(data.levelState.delta.levelAcquiredPaydata).Where(data => data.type == PayData.DataType.password).Count();
        int currentKeys = data.levelState.delta.physicalKeys.Count + data.levelState.delta.keycards.Count + numberPasswords;

        lootCount.text = $"{currentLoots}";
        dataCount.text = $"{currentData}";
        keyCount.text = $"{currentKeys}";
        ChangeTabCallback("objective");
        myCanvas.enabled = true;
    }

    public void ContinueButtonCallback() {
        GameManager.I.CloseMenu();
        GameManager.I.PlayUISound(closeSounds);
    }
    public void AbortButtonCallback() {
        GameManager.I.CloseMenu();
        GameManager.I.HandleObjectiveFailed();
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
        // foreach (Button button in tabButtons) {
        // }
        objectiveTabButton.colors = normalTabColors;
        mapTabButton.colors = normalTabColors;
        dataTabButton.colors = normalTabColors;
        keyTabButton.colors = normalTabColors;
        lootTabButton.colors = normalTabColors;
        target.colors = selectedTabColors;
    }
}
