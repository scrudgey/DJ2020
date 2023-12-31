using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Easings;
using UnityEngine;
public class CyberdeckUIController : MonoBehaviour {
    public RectTransform rectTransform;
    public Transform softwareContainer;
    public GameObject softwareButtonPrefab;

    [Header("buttons")]
    List<SoftwareButton> buttons;
    public SoftwareButton[] builtInButtons;
    [Header("hack progress")]
    public Transform hackProgressContainer;
    public GameObject hackProgressPrefab;
    NeoCyberNodeIndicator indicator;
    List<CyberdeckHackProgressBar> progressBars;
    Coroutine showCoroutine;
    bool shown;
    OverlayHandler handler;
    CyberGraph graph;
    void Start() {
        shown = false;
        ShowHideRect(false);
    }
    public void Initialize(OverlayHandler handler, CyberGraph graph) {
        this.handler = handler;
        this.graph = graph;
        PopulateSoftwareButtons();
        graph.NetworkActionsChanged += HandleNetworkActionChange;
        graph.NetworkActionUpdate += HandleNetworkActionUpdate;
    }
    void OnDestroy() {
        graph.NetworkActionsChanged -= HandleNetworkActionChange;
        graph.NetworkActionUpdate -= HandleNetworkActionUpdate;
    }
    public void Refresh(NeoCyberNodeIndicator indicator) {
        this.indicator = indicator;
        if (indicator == null) return;
        bool targetIsDatastore = indicator.node.type == CyberNodeType.datanode;
        bool targetIsLocked = indicator.node.lockLevel > 0;
        bool targetIsUnknown = indicator.node.visibility < NodeVisibility.mapped;
        bool dataIsStolen = indicator.node.dataStolen;
        foreach (SoftwareButton button in buttons.Concat(builtInButtons)) {
            switch (button.effect.type) {
                case SoftwareEffect.Type.scan:
                    button.button.interactable = targetIsUnknown;
                    break;
                case SoftwareEffect.Type.download:
                    button.button.interactable = targetIsDatastore && !targetIsLocked & !dataIsStolen;
                    break;
                case SoftwareEffect.Type.unlock:
                    button.button.interactable = targetIsLocked;
                    break;
                case SoftwareEffect.Type.compromise:
                    button.button.interactable = !targetIsLocked;
                    break;
            }
        }
    }

    public void Show() {
        if (!shown) {
            shown = true;
            ShowHideRect(true);
        }
    }
    public void Hide() {
        if (shown) {
            shown = false;
            ShowHideRect(false);
        }
    }
    void PopulateSoftwareButtons() {
        buttons = new List<SoftwareButton>();
        foreach (Transform child in softwareContainer) {
            Destroy(child.gameObject);
        }
        AddSoftwareButton(new SoftwareEffect() {
            type = SoftwareEffect.Type.compromise,
            level = 1,
            name = "crack"
        });
    }

    void AddSoftwareButton(SoftwareEffect effect) {
        GameObject obj = GameObject.Instantiate(softwareButtonPrefab);
        obj.transform.SetParent(softwareContainer, false);
        SoftwareButton button = obj.GetComponent<SoftwareButton>();
        button.effect = effect;
        button.Initialize(this);
        buttons.Add(button);
    }

    void ShowHideRect(bool show) {
        if (showCoroutine != null) {
            StopCoroutine(showCoroutine);
        }
        showCoroutine = StartCoroutine(MoveRect(show));
    }

    IEnumerator MoveRect(bool value) {
        float startPoint = value ? -422 : 5f;
        float endPoint = value ? 5f : -422;
        return Toolbox.Ease(null, 0.5f, startPoint, endPoint, PennerDoubleAnimation.ExpoEaseOut, (amount) => {
            Vector3 newPosition = rectTransform.anchoredPosition;
            newPosition.y = amount;
            rectTransform.anchoredPosition = newPosition;
        }, unscaledTime: true);
    }
    public void SoftwareButtonCallback(SoftwareButton button) {
        if (indicator == null) { return; }
        CyberGraph graph = GameManager.I.gameData.levelState.delta.cyberGraph;
        if (!graph.networkActions.ContainsKey(indicator.node) || graph.networkActions[indicator.node].Count() < 1) {
            NetworkAction networkAction = new NetworkAction() {
                title = "hacking...",
                effect = button.effect,
                lifetime = 2f,
                fromNode = null,
                toNode = indicator.node,
                timerRate = 1f,
            };
            // graph.networkActions.Add(networkAction);
            graph.AddNetworkAction(networkAction);
            GameManager.I.RefreshCyberGraph();
            handler.NodeSelectCallback(indicator);
        }

    }

    void HandleNetworkActionChange(Dictionary<CyberNode, List<NetworkAction>> networkActions) {
        PopulateHackIndicators(networkActions);
    }
    void PopulateHackIndicators(Dictionary<CyberNode, List<NetworkAction>> networkActions) {
        progressBars = new List<CyberdeckHackProgressBar>();
        foreach (Transform child in hackProgressContainer) {
            if (child.name == "bkg") continue;
            Destroy(child.gameObject);
        }
        foreach (KeyValuePair<CyberNode, List<NetworkAction>> kvp in networkActions) {
            foreach (NetworkAction networkAction in kvp.Value) {
                GameObject obj = GameObject.Instantiate(hackProgressPrefab);
                obj.transform.SetParent(hackProgressContainer, false);
                CyberdeckHackProgressBar progressBar = obj.GetComponent<CyberdeckHackProgressBar>();
                progressBar.Initialize(networkAction);
                progressBars.Add(progressBar);
            }
        }

    }
    void HandleNetworkActionUpdate(NetworkAction networkAction) {
        foreach (CyberdeckHackProgressBar progressBar in progressBars) {
            if (progressBar.networkAction == networkAction) {
                progressBar.HandleNetworkActionChange(networkAction);
            }
        }
    }
}
