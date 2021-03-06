using System.Collections;
using System.Collections.Generic;
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
    private Camera _cam;
    public UIColorSet powerOverlayColors;
    public UIColorSet cyberOverlayColors;
    public Camera cam {
        get { return _cam; }
        set {
            _cam = value;
            powerOverlay.cam = value;
        }
    }
    void Awake() {
        powerOverlay.colorSet = powerOverlayColors;
    }
    public void Bind() {
        // TODO: this is weird.
        GameManager.OnOverlayChange += HandleOverlayChange;
        GameManager.OnPowerGraphChange += RefreshPowerGraph;
        GameManager.OnCyberGraphChange += RefreshCyberGraph;

        // TODO: cyber
        RefreshPowerGraph(GameManager.I.gameData.levelData.powerGraph);
        RefreshCyberGraph(GameManager.I.gameData.levelData.cyberGraph);
        HandleOverlayChange(GameManager.I.activeOverlayType);
    }
    void OnDestroy() {
        GameManager.OnPowerGraphChange -= RefreshPowerGraph;
        GameManager.OnOverlayChange -= HandleOverlayChange;
    }

    public void RefreshPowerGraph(PowerGraph graph) {
        powerOverlay.cam = cam;
        powerOverlay.Refresh(graph);
    }
    public void RefreshCyberGraph(CyberGraph graph) {
        cyberOverlay.cam = cam;
        cyberOverlay.Refresh(graph);
    }
    public void HandleOverlayChange(OverlayType type) {
        // Debug.Log($"handling overlay change: {type}");
        switch (type) {
            case OverlayType.none:
            default:
                powerOverlay.gameObject.SetActive(false);
                cyberOverlay.gameObject.SetActive(false);
                titleText.text = "None";
                break;
            case OverlayType.power:
                outlineImage.color = powerOverlayColors.enabledColor;
                titleBoxImage.color = powerOverlayColors.enabledColor;
                titleText.color = powerOverlayColors.enabledColor;
                powerOverlay.gameObject.SetActive(true);
                cyberOverlay.gameObject.SetActive(false);
                titleText.text = "Power";
                break;
            case OverlayType.cyber:
                outlineImage.color = cyberOverlayColors.enabledColor;
                titleBoxImage.color = cyberOverlayColors.enabledColor;
                titleText.color = cyberOverlayColors.enabledColor;
                powerOverlay.gameObject.SetActive(false);
                cyberOverlay.gameObject.SetActive(true);
                titleText.text = "Network";
                break;
        }
        if (type == OverlayType.none) {
            outline.SetActive(false);
        } else {
            outline.SetActive(true);
        }
    }
}
