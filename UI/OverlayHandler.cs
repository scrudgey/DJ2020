using System.Collections;
using System.Collections.Generic;
using Easings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class UIColorSet {
    public Color enabledColor;
    public Color disabledColor;
    public Color deadColor;
}
public class OverlayHandler : MonoBehaviour {
    public GameObject outline;
    public Image outlineImage;
    public Image titleBoxImage;
    public TextMeshProUGUI titleText;
    public PowerOverlay powerOverlay;
    public CyberOverlay cyberOverlay;
    public AlarmOverlay alarmOverlay;
    private Camera _cam;
    public UIColorSet powerOverlayColors;
    public UIColorSet cyberOverlayColors;
    public UIColorSet alarmOverlayColors;
    public AudioClip[] overlayButtonSounds;
    public AudioSource audioSource;
    public INodeCameraProvider selectedNode;
    // public RectTransform infoPaneRect;
    public CyberNodeInfoPaneDisplay cyberInfoPaneDisplay;
    public PowerNodeInfoDisplay powerInfoPaneDisplay;
    public AlarmNodeInfoDisplay alarmInfoPaneDisplay;
    public NodeSelectionIndicator selectionIndicator;
    public UIController uIController;
    GameObject activeInfoPane;
    Coroutine showInfoRoutine;
    public Camera cam {
        get { return _cam; }
        set {
            _cam = value;
            powerOverlay.cam = value;
        }
    }
    void Awake() {
        powerOverlay.colorSet = powerOverlayColors;
        selectionIndicator.HideSelection();
    }
    public void Bind() {
        GameManager.OnOverlayChange += HandleOverlayChange;
        GameManager.OnPowerGraphChange += RefreshPowerGraph;
        GameManager.OnCyberGraphChange += RefreshCyberGraph;
        GameManager.OnAlarmGraphChange += RefreshAlarmGraph;

        powerOverlay.overlayHandler = this;
        cyberOverlay.overlayHandler = this;
        alarmOverlay.overlayHandler = this;

        RefreshPowerGraph(GameManager.I.gameData.levelState.delta.powerGraph);
        RefreshCyberGraph(GameManager.I.gameData.levelState.delta.cyberGraph);
        RefreshAlarmGraph(GameManager.I.gameData.levelState.delta.alarmGraph);
        HandleOverlayChange(GameManager.I.activeOverlayType, false);
    }
    void OnDestroy() {
        GameManager.OnOverlayChange -= HandleOverlayChange;
        GameManager.OnPowerGraphChange -= RefreshPowerGraph;
        GameManager.OnCyberGraphChange -= RefreshCyberGraph;
        GameManager.OnAlarmGraphChange -= RefreshAlarmGraph;
    }

    public void RefreshPowerGraph(PowerGraph graph) {
        powerOverlay.cam = cam;
        powerOverlay.Refresh(graph);
    }
    public void RefreshCyberGraph(CyberGraph graph) {
        cyberOverlay.cam = cam;
        cyberOverlay.Refresh(graph);
    }
    public void RefreshAlarmGraph(AlarmGraph graph) {
        alarmOverlay.cam = cam;
        alarmOverlay.Refresh(graph);
    }
    public void HandleOverlayChange(OverlayType type, bool enabled) {
        switch (type) {
            case OverlayType.none:
            default:
                powerOverlay.gameObject.SetActive(false);
                cyberOverlay.gameObject.SetActive(false);
                alarmOverlay.gameObject.SetActive(false);
                titleText.text = "None";
                break;
            case OverlayType.power:
                outlineImage.color = powerOverlayColors.enabledColor;
                titleText.color = powerOverlayColors.enabledColor;
                powerOverlay.gameObject.SetActive(true);
                cyberOverlay.gameObject.SetActive(false);
                alarmOverlay.gameObject.SetActive(false);
                titleText.text = "Power";
                break;
            case OverlayType.cyber:
                outlineImage.color = cyberOverlayColors.enabledColor;
                titleText.color = cyberOverlayColors.enabledColor;
                powerOverlay.gameObject.SetActive(false);
                cyberOverlay.gameObject.SetActive(true);
                alarmOverlay.gameObject.SetActive(false);
                titleText.text = "Network";
                break;
            case OverlayType.alarm:
                outlineImage.color = alarmOverlayColors.enabledColor;
                titleText.color = alarmOverlayColors.enabledColor;
                powerOverlay.gameObject.SetActive(false);
                cyberOverlay.gameObject.SetActive(false);
                alarmOverlay.gameObject.SetActive(true);
                titleText.text = "Alarm";
                break;
        }
        if (!enabled) {
            powerOverlay.gameObject.SetActive(false);
            cyberOverlay.gameObject.SetActive(false);
            alarmOverlay.gameObject.SetActive(false);
            outlineImage.enabled = false;
        } else if (type == OverlayType.none) {
            outlineImage.enabled = false;
        } else {
            outlineImage.enabled = true;
        }
        ShowInfoPane(null);
        activeInfoPane = null;
    }

    public void NextOverlayButton() {
        int overlayIndex = (int)GameManager.I.activeOverlayType + 1;
        if (overlayIndex > 3) {
            overlayIndex = 0;
        }
        OverlayType newOverlay = (OverlayType)overlayIndex;
        GameManager.I.SetOverlay(newOverlay);
        Toolbox.RandomizeOneShot(audioSource, overlayButtonSounds);
    }
    public void PreviousOverlayButton() {
        int overlayIndex = (int)GameManager.I.activeOverlayType - 1;
        if (overlayIndex < 0) {
            overlayIndex = 3;
        }
        OverlayType newOverlay = (OverlayType)overlayIndex;
        GameManager.I.SetOverlay(newOverlay);
        Toolbox.RandomizeOneShot(audioSource, overlayButtonSounds, randomPitchWidth: 0.05f);
    }
    public void NodeSelectCallback<T, U>(NodeIndicator<T, U> indicator) where T : Node<T> where U : Graph<T, U> {
        selectedNode = indicator;
        selectionIndicator.ActivateSelection(indicator);
        cyberOverlay.NeighborButtonMouseExit();
        powerOverlay.NeighborButtonMouseExit();
        alarmOverlay.NeighborButtonMouseExit();
        GameObject infoPane = null;
        switch (indicator) {
            case NeoCyberNodeIndicator cybernode:
                cyberInfoPaneDisplay.Configure(cybernode, GameManager.I.gameData.levelState.delta.cyberGraph, cyberOverlay);
                infoPane = cyberInfoPaneDisplay.gameObject;
                break;
            case PowerNodeIndicator powernode:
                powerInfoPaneDisplay.Configure(powernode, GameManager.I.gameData.levelState.delta.powerGraph, powerOverlay);
                infoPane = powerInfoPaneDisplay.gameObject;
                break;
            case AlarmNodeIndicator alarmnode:
                alarmInfoPaneDisplay.Configure(alarmnode, GameManager.I.gameData.levelState.delta.alarmGraph, alarmOverlay);
                infoPane = alarmInfoPaneDisplay.gameObject;
                break;
        }
        if (infoPane != activeInfoPane) {
            ShowInfoPane(infoPane);
            activeInfoPane = infoPane;
        }
    }
    public void InfoPaneDoneButtonCallback() {
        // infoPaneDisplayed = false;
        activeInfoPane = null;
        ShowInfoPane(null);
        selectedNode = null;
        selectionIndicator.HideSelection();
    }
    public void ShowInfoPane(GameObject infoPane) {
        if (showInfoRoutine != null) {
            StopCoroutine(showInfoRoutine);
        }

        if (infoPane != activeInfoPane) {
            if (activeInfoPane != null) {
                showInfoRoutine = StartCoroutine(Toolbox.ChainCoroutines(
                    EaseInInfoPane(false, activeInfoPane),
                    EaseInInfoPane(true, infoPane)
                ));
            } else {
                showInfoRoutine = StartCoroutine(EaseInInfoPane(true, infoPane));
            }
        } else {
            showInfoRoutine = StartCoroutine(EaseInInfoPane(true, infoPane));
        }
    }

    IEnumerator EaseInInfoPane(bool value, GameObject pane) {
        RectTransform infoPaneRect = pane.GetComponent<RectTransform>();

        float y = infoPaneRect.anchoredPosition.y;
        float startX = infoPaneRect.anchoredPosition.x;
        float finishX = value ? -550 : -14;
        yield return Toolbox.Ease(null, 0.5f, startX, finishX, PennerDoubleAnimation.ExpoEaseOut, (amount) => {
            infoPaneRect.anchoredPosition = new Vector2(amount, y);
        }, unscaledTime: true);
    }


}
