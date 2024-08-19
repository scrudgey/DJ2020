using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Easings;
using UnityEngine;
public class CyberdeckUIController : MonoBehaviour {
    public CyberOverlay cyberOverlay;
    public OverlayHandler overlayHandler;
    public RectTransform rectTransform;
    public Transform softwareContainer;
    public GameObject softwareButtonPrefab;
    [Header("sound effects")]
    public AudioClip[] startDownloadSound;

    [Header("buttons")]
    List<SoftwareButton> buttons;
    public SoftwareButton[] builtInButtons;
    [Header("hack progress")]
    public Transform hackProgressContainer;
    public GameObject hackProgressPrefab;
    NeoCyberNodeIndicator indicator;
    List<CyberdeckHackProgressBar> progressBars;
    Coroutine showCoroutine;
    // bool shown;
    enum State { hidden, progressOnly, full }
    State state;
    OverlayHandler handler;
    CyberGraph graph;
    void Start() {
        state = State.hidden;
    }
    public void Initialize(OverlayHandler handler, CyberGraph graph) {
        this.handler = handler;
        this.graph = graph;
        PopulateSoftwareButtons();
        PopulateHackIndicators(new Dictionary<CyberNode, List<NetworkAction>>());
    }
    public void Refresh(NeoCyberNodeIndicator indicator) {
        this.indicator = indicator;
        if (indicator == null) return;
    }

    public void Show() {
        if (state != State.full) {
            state = State.full;
            ShowHideRect(true);
        }
    }
    public void Hide() {
        if (state != State.hidden) {
            state = State.hidden;
            ShowHideRect(false);
        }
    }
    public void ShowOnlyProgress() {
        if (state != State.progressOnly) {
            state = State.progressOnly;
            ShowHideRect(true, progressOnly: true);
        }
    }
    void PopulateSoftwareButtons() {
        buttons = new List<SoftwareButton>();
        foreach (Transform child in softwareContainer) {
            Destroy(child.gameObject);
        }
        AddSoftwareButton(new SoftwareEffect() {
            type = SoftwareEffect.Type.compromise,
            // level = 1,
            // name = "crack.exe"
        });
    }

    void AddSoftwareButton(SoftwareEffect effect) {
        GameObject obj = GameObject.Instantiate(softwareButtonPrefab);
        obj.transform.SetParent(softwareContainer, false);
        SoftwareButton button = obj.GetComponent<SoftwareButton>();
        // button.effect = effect;
        // button.Initialize(effect);
        buttons.Add(button);
    }

    void ShowHideRect(bool show, bool progressOnly = false) {
        if (showCoroutine != null) {
            StopCoroutine(showCoroutine);
        }
        showCoroutine = StartCoroutine(MoveRect(show, progressOnly: progressOnly));
    }

    IEnumerator MoveRect(bool value, bool progressOnly = false) {
        float startPoint = rectTransform.anchoredPosition.y;
        float endPoint;
        if (progressOnly) {
            endPoint = -176;
        } else {
            endPoint = value ? 5f : (-1f * rectTransform.rect.height - 20f);
        }
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
            // NetworkAction networkAction = button.GetNetworkAction(indicator.node, graph);
            // graph.AddNetworkAction(networkAction);
            GameManager.I.RefreshCyberGraph();
            handler.NodeSelectCallback(indicator);
            Toolbox.RandomizeOneShot(overlayHandler.audioSource, startDownloadSound);
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

    public void SoftwareButtonMouseover(SoftwareButton button) {
        // cyberOverlay.
        CyberNode node = overlayHandler.selectedCyberNodeIndicator.node;
        CyberGraph graph = GameManager.I.gameData.levelState.delta.cyberGraph;
        // cyberOverlay.DrawThreat(button.GetNetworkAction(node, graph));
    }
    public void SoftwareButtonMouseExit(SoftwareButton button) {
        CyberNode node = overlayHandler.selectedCyberNodeIndicator.node;
        CyberGraph graph = GameManager.I.gameData.levelState.delta.cyberGraph;
        // cyberOverlay.EraseThreat(button.GetNetworkAction(node, graph));
    }
}
