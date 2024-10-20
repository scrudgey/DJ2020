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
    public INodeCameraProvider mousedOverNode;
    public CyberNodeInfoPaneDisplay cyberInfoPaneDisplay;
    public PowerNodeInfoDisplay powerInfoPaneDisplay;
    public AlarmNodeInfoDisplay alarmInfoPaneDisplay;
    [Header("info pane rects")]
    public RectTransform cyberNodeInfoRect;
    public RectTransform powerNodeInfoRect;
    public RectTransform alarmNodeInfoRect;
    public NodeSelectionIndicator selectionIndicator;
    public NodeMouseOverIndicator mouseOverIndicator;
    public UIController uIController;
    public static Action<INodeCameraProvider> OnSelectedNodeChange;

    public NeoCyberNodeIndicator selectedCyberNodeIndicator;
    public AlarmNodeIndicator selectedAlarmNodeIndicator;
    public PowerNodeIndicator selectedPowerNodeIndicator;

    public NeoCyberNodeIndicator mousedOverCyberNodeIndicator;
    public AlarmNodeIndicator mousedOverAlarmNodeIndicator;
    public PowerNodeIndicator mousedOverPowerNodeIndicator;
    [Header("sound effects")]
    public AudioClip[] mouseOverSound;
    public AudioClip[] nodeSelectSound;
    public AudioClip[] selectHackOriginSound;
    [Header("cyberdeck")]
    public CyberdeckUIController cyberdeckController;
    public NeoCyberNodeIndicator selectedHackOrigin;
    public List<CyberNode> hackOriginToPathTarget;
    public bool overrideDisconnectButton;
    public CanvasGroup canvasGroup;
    RectTransform activeInfoPane;
    Coroutine showInfoRoutine;
    bool bound;
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
        if (!bound) {
            bound = true;
            GameManager.OnOverlayChange += HandleOverlayChange;
            GameManager.OnPowerGraphChange += RefreshPowerGraph;
            GameManager.OnCyberGraphChange += RefreshCyberGraph;
            GameManager.OnAlarmGraphChange += RefreshAlarmGraph;
        }

        powerOverlay.overlayHandler = this;
        cyberOverlay.overlayHandler = this;
        alarmOverlay.overlayHandler = this;

        if (GameManager.I.gameData.levelState != null) {
            cyberdeckController.Initialize(this, GameManager.I.gameData.levelState?.delta.cyberGraph);
            RefreshPowerGraph(GameManager.I.gameData.levelState.delta.powerGraph);
            RefreshCyberGraph(GameManager.I.gameData.levelState.delta.cyberGraph);
            RefreshAlarmGraph(GameManager.I.gameData.levelState.delta.alarmGraph);
        }

        HandleOverlayChange(GameManager.I.activeOverlayType, false);
    }
    public void ClearAllIndicatorsAndEdges() {
        powerOverlay.ClearAllIndicatorsAndEdges();
        cyberOverlay.ClearAllIndicatorsAndEdges();
        alarmOverlay.ClearAllIndicatorsAndEdges();
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
        RefreshPowerInfoDisplays();
        SetDiscoveryText(GameManager.I.gameData.levelState.delta.powerGraph);
    }
    public void RefreshPowerInfoDisplays() {
        if (selectedPowerNodeIndicator != null) {
            powerInfoPaneDisplay.Configure(selectedPowerNodeIndicator, GameManager.I.gameData.levelState.delta.powerGraph, powerOverlay);
        } else if (mousedOverPowerNodeIndicator != null) {
            powerInfoPaneDisplay.Configure(mousedOverPowerNodeIndicator, GameManager.I.gameData.levelState.delta.powerGraph, powerOverlay);
        }
    }
    public void RefreshCyberGraph(CyberGraph graph) {
        cyberOverlay.cam = cam;
        UpdateHackPath();
        cyberOverlay.Refresh(graph);
        cyberOverlay.RefreshHackTerminal();
        RefreshCyberInfoDisplays();
        // HandleCyberDeckChange(selectedCyberNodeIndicator);
        SetDiscoveryText(GameManager.I.gameData.levelState.delta.cyberGraph);
    }
    public void RefreshCyberInfoDisplays() {
        if (selectedCyberNodeIndicator != null) {
            cyberdeckController.Refresh(selectedCyberNodeIndicator);
            cyberInfoPaneDisplay.Configure(selectedCyberNodeIndicator, GameManager.I.gameData.levelState.delta.cyberGraph, cyberOverlay);
        } else if (mousedOverCyberNodeIndicator != null) {
            cyberInfoPaneDisplay.Configure(mousedOverCyberNodeIndicator, GameManager.I.gameData.levelState.delta.cyberGraph, cyberOverlay);
        }
    }
    public void RefreshAlarmGraph(AlarmGraph graph) {
        alarmOverlay.cam = cam;
        alarmOverlay.Refresh(graph);
        RefreshAlarmInfoDisplays();
        SetDiscoveryText(GameManager.I.gameData.levelState.delta.alarmGraph);
    }
    public void RefreshAlarmInfoDisplays() {
        if (selectedAlarmNodeIndicator != null) {
            alarmInfoPaneDisplay.Configure(selectedAlarmNodeIndicator, GameManager.I.gameData.levelState.delta.alarmGraph, alarmOverlay);
        } else if (mousedOverAlarmNodeIndicator != null) {
            alarmInfoPaneDisplay.Configure(mousedOverAlarmNodeIndicator, GameManager.I.gameData.levelState.delta.alarmGraph, alarmOverlay);
        }
    }
    public void HandleOverlayChange(OverlayType type, bool enabled) {
        selectedAlarmNodeIndicator = null;
        selectedCyberNodeIndicator = null;
        selectedPowerNodeIndicator = null;
        HackOriginSelectCallback(null);
        NodeMouseExitCallback();

        GameManager.I.playerManualHacker.Disconnect();
        CutsceneManager.I.HandleTrigger("disconnect");

        switch (type) {
            case OverlayType.none:
            default:
                if (GameManager.I.playerItemHandler.activeItem is CyberDeck) {
                    GameManager.I.SetOverlay(OverlayType.limitedCyber);
                    return;
                } else {
                    if (selectedHackOrigin != null) {
                        selectedHackOrigin.SetHackOrigin(false);
                    }
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
                nodeDiscoveryBox.SetActive(false);
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
                powerOverlay.OnOverlayActivate();
                nodeDiscoveryBox.SetActive(true);
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
                cyberOverlay.OnOverlayActivate();
                nodeDiscoveryBox.SetActive(true);
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
                alarmOverlay.OnOverlayActivate();
                nodeDiscoveryBox.SetActive(true);
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
        int knownNodes = graph.nodes.Values.Where(node => node.visibility >= NodeVisibility.mystery).Count();
        int totalNodes = graph.nodes.Count();
        nodesDiscoveredText.text = $"discovered: {knownNodes}/{totalNodes}";
    }

    public void NextOverlayButton() {
        int overlayIndex;
        if (GameManager.I.activeOverlayType == OverlayType.limitedCyber) {
            overlayIndex = 1;
        } else {
            overlayIndex = (int)GameManager.I.activeOverlayType + 1;
        }
        if (overlayIndex > 3) {
            overlayIndex = 0;
        }
        OverlayType newOverlay = (OverlayType)overlayIndex;
        GameManager.I.SetOverlay(newOverlay);
        Toolbox.RandomizeOneShot(audioSource, overlayButtonSounds);
        CutsceneManager.I.HandleTrigger($"overlay_change_{overlayIndex}");
    }
    public void PreviousOverlayButton() {
        int overlayIndex;
        if (GameManager.I.activeOverlayType == OverlayType.limitedCyber) {
            overlayIndex = 3;
        } else {
            overlayIndex = (int)GameManager.I.activeOverlayType - 1;
        }
        if (overlayIndex < 0) {
            overlayIndex = 3;
        }
        OverlayType newOverlay = (OverlayType)overlayIndex;
        GameManager.I.SetOverlay(newOverlay);
        Toolbox.RandomizeOneShot(audioSource, overlayButtonSounds, randomPitchWidth: 0.05f);
        CutsceneManager.I.HandleTrigger($"overlay_change_{overlayIndex}");
    }
    public void NodeSelectCallback<T, U>(NodeIndicator<T, U> indicator) where T : Node<T> where U : Graph<T, U> {
        if (selectedNode != indicator) {
            Toolbox.RandomizeOneShot(audioSource, nodeSelectSound);
        }
        SetSelectedNode(indicator); // sets node and notifies clearsighter
        selectionIndicator.ActivateSelection(indicator);
        mouseOverIndicator.HideSelection();
        switch (indicator) {
            case NeoCyberNodeIndicator cybernode:
                selectedCyberNodeIndicator = cybernode;
                if (GameManager.I.playerManualHacker.deployed && GameManager.I.playerManualHacker.targetNode == null) {
                    GameManager.I.playerManualHacker.Connect(cybernode.node);
                }
                RefreshCyberGraph(GameManager.I.gameData.levelState.delta.cyberGraph);
                break;
            case PowerNodeIndicator powernode:
                selectedPowerNodeIndicator = powernode;
                RefreshPowerGraph(GameManager.I.gameData.levelState.delta.powerGraph);
                break;
            case AlarmNodeIndicator alarmnode:
                selectedAlarmNodeIndicator = alarmnode;
                RefreshAlarmGraph(GameManager.I.gameData.levelState.delta.alarmGraph);
                break;
        }
        ShowInfoPaneForIndicator(indicator);
        CutsceneManager.I.HandleTrigger($"node_select_{indicator.node.nodeTitle}");
    }

    void ShowInfoPaneForIndicator<T, U>(NodeIndicator<T, U> indicator) where T : Node<T> where U : Graph<T, U> {
        RectTransform infoPane = null;
        switch (indicator) {
            case NeoCyberNodeIndicator cybernode:
                infoPane = cyberNodeInfoRect;
                break;
            case PowerNodeIndicator powernode:
                infoPane = powerNodeInfoRect;
                break;
            case AlarmNodeIndicator alarmnode:
                infoPane = alarmNodeInfoRect;
                break;
            default:
                infoPane = null;
                break;
        }
        Debug.Log($"show info pane: {infoPane} {activeInfoPane}");
        if (infoPane != activeInfoPane) {
            ShowInfoPane(infoPane);
            activeInfoPane = infoPane;
        }
    }

    public void HackOriginSelectCallback(NeoCyberNodeIndicator newOrigin) {
        if (newOrigin == selectedHackOrigin) return;
        if (selectedHackOrigin != null) {
            selectedHackOrigin.SetHackOrigin(false);
        }
        selectedHackOrigin = newOrigin;
        if (selectedHackOrigin != null) {
            Toolbox.RandomizeOneShot(audioSource, selectHackOriginSound);
            selectedHackOrigin.SetHackOrigin(true);
        }
        RefreshCyberGraph(GameManager.I.gameData.levelState.delta.cyberGraph);
    }

    void UpdateHackPath() {
        // TODO: shortest path
        if (selectedHackOrigin == null || selectedCyberNodeIndicator == null || selectedCyberNodeIndicator.node.nodeTitle == "WAN") {
            hackOriginToPathTarget = new List<CyberNode>();
        } else {
            hackOriginToPathTarget = GameManager.I.gameData.levelState.delta.cyberGraph.GetPath(selectedHackOrigin.node, selectedCyberNodeIndicator.node);
            if (hackOriginToPathTarget.Count > 3) {
                hackOriginToPathTarget = new List<CyberNode>();
                // selectedHackOrigin = null;
                HackOriginSelectCallback(null);
            }
        }
    }

    // void HandleCyberDeckChange(NeoCyberNodeIndicator cyberIndicator) {
    //     if (cyberIndicator == null) {
    //         // MaybeHideCyberdeck();
    //     } else {
    //         // CyberNode node = cyberIndicator.node;
    //         // MaybeHideCyberdeck();
    //     }
    // }
    // void MaybeHideCyberdeck() {
    //     if (GameManager.I.gameData.levelState.delta.cyberGraph.ActiveNetworkActions().Count > 0) {
    //         cyberdeckController.ShowOnlyProgress();
    //     } else {
    //         cyberdeckController.Hide();
    //     }
    // }
    public void NodeMouseOverCallback<T, U>(NodeIndicator<T, U> indicator) where T : Node<T> where U : Graph<T, U> {
        mousedOverNode = indicator;

        cyberOverlay.NeighborButtonMouseExit();
        powerOverlay.NeighborButtonMouseExit();
        alarmOverlay.NeighborButtonMouseExit();

        if (indicator != selectedNode && indicator != selectedHackOrigin) {
            mouseOverIndicator.ActivateSelection(indicator);
            Toolbox.RandomizeOneShot(audioSource, mouseOverSound);
        }

        mousedOverAlarmNodeIndicator = null;
        mousedOverCyberNodeIndicator = null;
        mousedOverPowerNodeIndicator = null;
        if (selectedNode == null) {
            switch (indicator) {
                case NeoCyberNodeIndicator cybernode:
                    mousedOverCyberNodeIndicator = cybernode;
                    RefreshCyberInfoDisplays();
                    break;
                case PowerNodeIndicator powernode:
                    mousedOverPowerNodeIndicator = powernode;
                    RefreshPowerInfoDisplays();
                    break;
                case AlarmNodeIndicator alarmnode:
                    mousedOverAlarmNodeIndicator = alarmnode;
                    RefreshAlarmInfoDisplays();
                    break;
            }
            ShowInfoPaneForIndicator(indicator);
        }
    }

    void SetSelectedNode(INodeCameraProvider selected) {
        selectedNode = selected;
        if (overrideDisconnectButton) {
            closeButtonObject.SetActive(false);
        } else {
            closeButtonObject.SetActive(selected != null);
        }
        OnSelectedNodeChange?.Invoke(selected);
    }
    public void NodeMouseExitCallback() {
        mouseOverIndicator.HideSelection();
        mousedOverAlarmNodeIndicator = null;
        mousedOverCyberNodeIndicator = null;
        mousedOverPowerNodeIndicator = null;
        if (selectedNode == null) {
            ShowInfoPane(null);
        }
    }
    public void InfoPaneDoneButtonCallback() {
        ShowInfoPane(null);
        activeInfoPane = null;
        SetSelectedNode(null);
        selectedAlarmNodeIndicator = null;
        selectedCyberNodeIndicator = null;
        selectedPowerNodeIndicator = null;
        if (GameManager.I.playerManualHacker.deployed) {
            // selectedCyberNodeIndicator = 
            foreach (NeoCyberNodeIndicator indicator in GameObject.FindObjectsOfType<NeoCyberNodeIndicator>()) {
                if (indicator.node == GameManager.I.playerManualHacker.targetNode) {
                    selectedCyberNodeIndicator = indicator;
                    break;
                }
            }
        }
        selectionIndicator.HideSelection();
        mouseOverIndicator.HideSelection();
        RefreshCyberGraph(GameManager.I.gameData.levelState.delta.cyberGraph);
    }
    public void ShowInfoPane(RectTransform infoPane) {
        if (activeInfoPane == infoPane) return;

        if (showInfoRoutine != null) {
            StopCoroutine(showInfoRoutine);
        }
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
        activeInfoPane = infoPane;
    }

    IEnumerator EaseInInfoPane(bool value, RectTransform infoPaneRect) {
        float y = infoPaneRect.anchoredPosition.y;
        float startX = infoPaneRect.anchoredPosition.x;
        float finishX = value ? -570 : -14;
        // Debug.Log($"[info pane] ease info pane: {value} {infoPaneRect} {startX}->{finishX}");
        yield return Toolbox.Ease(null, 0.5f, startX, finishX, PennerDoubleAnimation.ExpoEaseOut, (amount) => {
            infoPaneRect.anchoredPosition = new Vector2(amount, y);
        }, unscaledTime: true);
    }

    public void CloseButtonCallback() {
        SetSelectedNode(null);
        GameManager.I.SetOverlay(OverlayType.none);
        HackOriginSelectCallback(null);
        if (GameManager.I != null && GameManager.I.playerManualHacker != null)
            GameManager.I.playerManualHacker.Disconnect();
        CutsceneManager.I.HandleTrigger("disconnect");
        RefreshCyberInfoDisplays();
        RefreshPowerInfoDisplays();
        RefreshAlarmInfoDisplays();
        NodeMouseExitCallback();
    }
}
