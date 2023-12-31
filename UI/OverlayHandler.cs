using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Easings;
using Items;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;
[System.Serializable]
public class UIColorSet {
    public Color enabledColor;
    public Color disabledColor;
    public Color deadColor;
}
public class OverlayHandler : MonoBehaviour {
    [Header("aspects")]
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
    public TextMeshProUGUI nodesDiscoveredText;
    public GameObject nodeDiscoveryBox;
    public GameObject closeButtonObject;
    public Image[] colorImages;
    public TextMeshProUGUI[] colorTexts;
    [Header("details")]
    public AudioSource audioSource;
    public INodeCameraProvider selectedNode;
    public INodeCameraProvider mouseOverNode;
    public CyberNodeInfoPaneDisplay cyberInfoPaneDisplay;
    public PowerNodeInfoDisplay powerInfoPaneDisplay;
    public AlarmNodeInfoDisplay alarmInfoPaneDisplay;
    public NodeSelectionIndicator selectionIndicator;
    public NodeMouseOverIndicator mouseOverIndicator;
    public UIController uIController;
    public static Action<INodeCameraProvider> OnSelectedNodeChange;

    NeoCyberNodeIndicator selectedCyberNodeIndicator;
    AlarmNodeIndicator selectedAlarmNodeIndicator;
    PowerNodeIndicator selectedPowerNodeIndicator;
    [Header("cyberdeck")]
    public CyberdeckUIController cyberdeckController;

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
        mouseOverIndicator.HideSelection();
        cyberdeckController.Hide();
    }
    public void Bind() {
        GameManager.OnOverlayChange += HandleOverlayChange;
        GameManager.OnPowerGraphChange += RefreshPowerGraph;
        GameManager.OnCyberGraphChange += RefreshCyberGraph;
        GameManager.OnAlarmGraphChange += RefreshAlarmGraph;

        powerOverlay.overlayHandler = this;
        cyberOverlay.overlayHandler = this;
        alarmOverlay.overlayHandler = this;

        cyberdeckController.Initialize(this, GameManager.I.gameData.levelState.delta.cyberGraph);

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
        if (selectedPowerNodeIndicator != null)
            powerInfoPaneDisplay.Configure(selectedPowerNodeIndicator, GameManager.I.gameData.levelState.delta.powerGraph, powerOverlay);
    }
    public void RefreshCyberGraph(CyberGraph graph) {
        cyberOverlay.cam = cam;
        cyberOverlay.Refresh(graph);
        if (selectedCyberNodeIndicator != null) {
            cyberdeckController.Refresh(selectedCyberNodeIndicator);
            cyberInfoPaneDisplay.Configure(selectedCyberNodeIndicator, GameManager.I.gameData.levelState.delta.cyberGraph, cyberOverlay);
        }
    }
    public void RefreshAlarmGraph(AlarmGraph graph) {
        alarmOverlay.cam = cam;
        alarmOverlay.Refresh(graph);
        if (selectedAlarmNodeIndicator != null)
            alarmInfoPaneDisplay.Configure(selectedAlarmNodeIndicator, GameManager.I.gameData.levelState.delta.alarmGraph, alarmOverlay);
    }
    public void HandleOverlayChange(OverlayType type, bool enabled) {
        switch (type) {
            case OverlayType.none:
            default:
                if (GameManager.I.playerItemHandler.activeItem is CyberDeck) {
                    GameManager.I.SetOverlay(OverlayType.limitedCyber);
                    return;
                }
                foreach (Image image in colorImages) {
                    image.color = powerOverlayColors.enabledColor;
                }
                foreach (TextMeshProUGUI text in colorTexts) {
                    text.color = powerOverlayColors.enabledColor;
                }
                nodeDiscoveryBox.SetActive(false);
                powerOverlay.gameObject.SetActive(false);
                cyberOverlay.gameObject.SetActive(false);
                alarmOverlay.gameObject.SetActive(false);
                titleText.text = "None";
                closeButtonObject.SetActive(false);
                cyberdeckController.Hide();
                break;
            case OverlayType.power:
                foreach (Image image in colorImages) {
                    image.color = powerOverlayColors.enabledColor;
                }
                foreach (TextMeshProUGUI text in colorTexts) {
                    text.color = powerOverlayColors.enabledColor;
                }
                powerOverlay.gameObject.SetActive(true);
                cyberOverlay.gameObject.SetActive(false);
                alarmOverlay.gameObject.SetActive(false);
                titleText.text = "Power";
                SetDiscoveryText(GameManager.I.gameData.levelState.delta.powerGraph);
                powerOverlay.OnOverlayActivate();
                closeButtonObject.SetActive(true);
                cyberdeckController.Hide();
                break;
            case OverlayType.limitedCyber:
            case OverlayType.cyber:
                foreach (Image image in colorImages) {
                    image.color = cyberOverlayColors.enabledColor;
                }
                foreach (TextMeshProUGUI text in colorTexts) {
                    text.color = cyberOverlayColors.enabledColor;
                }
                powerOverlay.gameObject.SetActive(false);
                cyberOverlay.gameObject.SetActive(true);
                alarmOverlay.gameObject.SetActive(false);
                titleText.text = type == OverlayType.cyber ? "Network" : "None";
                SetDiscoveryText(GameManager.I.gameData.levelState.delta.cyberGraph);
                cyberOverlay.OnOverlayActivate();
                closeButtonObject.SetActive(true);
                break;
            case OverlayType.alarm:
                foreach (Image image in colorImages) {
                    image.color = alarmOverlayColors.enabledColor;
                }
                foreach (TextMeshProUGUI text in colorTexts) {
                    text.color = alarmOverlayColors.enabledColor;
                }
                powerOverlay.gameObject.SetActive(false);
                cyberOverlay.gameObject.SetActive(false);
                alarmOverlay.gameObject.SetActive(true);
                titleText.text = "Alarm";
                SetDiscoveryText(GameManager.I.gameData.levelState.delta.alarmGraph);
                alarmOverlay.OnOverlayActivate();
                closeButtonObject.SetActive(true);
                cyberdeckController.Hide();
                break;
        }

        if (!enabled) {
            powerOverlay.gameObject.SetActive(false);
            cyberOverlay.gameObject.SetActive(false);
            alarmOverlay.gameObject.SetActive(false);
            outlineImage.enabled = false;
        } else if (type == OverlayType.none || type == OverlayType.limitedCyber) {
            outlineImage.enabled = false;
        } else {
            outlineImage.enabled = true;
        }
        selectionIndicator.HideSelection();
        mouseOverIndicator.HideSelection();
        SetSelectedNode(null);
        ShowInfoPane(null);
        activeInfoPane = null;
    }

    public void SetDiscoveryText<T, U>(Graph<T, U> graph) where U : Graph<T, U> where T : Node<T> {
        nodeDiscoveryBox.SetActive(true);
        int knownNodes = graph.nodes.Values.Where(node => node.visibility > NodeVisibility.mystery).Count();
        int totalNodes = graph.nodes.Count();
        nodesDiscoveredText.text = $"discovered: {knownNodes}/{totalNodes}";
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
        SetSelectedNode(indicator);
        selectionIndicator.ActivateSelection(indicator);
        mouseOverIndicator.HideSelection();
        cyberOverlay.NeighborButtonMouseExit();
        powerOverlay.NeighborButtonMouseExit();
        alarmOverlay.NeighborButtonMouseExit();
        GameObject infoPane = null;
        switch (indicator) {
            case NeoCyberNodeIndicator cybernode:
                selectedCyberNodeIndicator = cybernode;
                if (GameManager.I.playerManualHacker.deployed) {
                    GameManager.I.playerManualHacker.Connect(cybernode.node);
                }
                infoPane = cyberInfoPaneDisplay.gameObject;
                RefreshCyberGraph(GameManager.I.gameData.levelState.delta.cyberGraph);
                HandleCyberDeckChange(cybernode);
                break;
            case PowerNodeIndicator powernode:
                selectedPowerNodeIndicator = powernode;
                infoPane = powerInfoPaneDisplay.gameObject;
                RefreshPowerGraph(GameManager.I.gameData.levelState.delta.powerGraph);
                break;
            case AlarmNodeIndicator alarmnode:
                selectedAlarmNodeIndicator = alarmnode;
                infoPane = alarmInfoPaneDisplay.gameObject;
                RefreshAlarmGraph(GameManager.I.gameData.levelState.delta.alarmGraph);
                break;
        }
        if (infoPane != activeInfoPane) {
            ShowInfoPane(infoPane);
            activeInfoPane = infoPane;
        }
    }

    void HandleCyberDeckChange(NeoCyberNodeIndicator cyberIndicator) {
        if (cyberIndicator.node.getStatus() >= CyberNodeStatus.vulnerable) {
            cyberdeckController.Show();
        } else {
            cyberdeckController.Hide();
        }
    }
    public void NodeMouseOverCallback<T, U>(NodeIndicator<T, U> indicator) where T : Node<T> where U : Graph<T, U> {
        if (indicator != selectedNode) {
            mouseOverIndicator.ActivateSelection(indicator);
        }
    }

    void SetSelectedNode(INodeCameraProvider selected) {
        selectedNode = selected;
        OnSelectedNodeChange?.Invoke(selected);
    }
    public void NodeMouseExitCallback() {
        mouseOverNode = null;
        mouseOverIndicator.HideSelection();
    }
    public void InfoPaneDoneButtonCallback() {
        ShowInfoPane(null);
        activeInfoPane = null;
        SetSelectedNode(null);
        selectionIndicator.HideSelection();
        mouseOverIndicator.HideSelection();
    }
    public void ShowInfoPane(GameObject infoPane) {
        if (showInfoRoutine != null) {
            StopCoroutine(showInfoRoutine);
        }
        Debug.Log($"show info pane: {infoPane}");
        if (infoPane != activeInfoPane) {
            if (infoPane == null) {
                showInfoRoutine = StartCoroutine(EaseInInfoPane(false, activeInfoPane));
            } else if (activeInfoPane != null) {
                showInfoRoutine = StartCoroutine(Toolbox.ChainCoroutines(
                    EaseInInfoPane(false, activeInfoPane),
                    EaseInInfoPane(true, infoPane)
                ));
            } else {
                showInfoRoutine = StartCoroutine(EaseInInfoPane(true, infoPane));
            }
        } else if (infoPane != null) {
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

    public void CloseButtonCallback() {
        GameManager.I.SetOverlay(OverlayType.none);
    }
}
