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
        GameManager.OnPowerGraphChange += RefreshPowerGraph;
        GameManager.OnOverlayChange += HandleOverlayChange;
        RefreshPowerGraph(GameManager.I.gameData.levelData.powerGraph);
        HandleOverlayChange(OverlayType.none);
    }
    void OnDestroy() {
        GameManager.OnPowerGraphChange -= RefreshPowerGraph;
        GameManager.OnOverlayChange -= HandleOverlayChange;
    }

    public void RefreshPowerGraph(PowerGraph graph) {
        powerOverlay.Refresh(graph);
        powerOverlay.cam = cam;
    }

    public void HandleOverlayChange(OverlayType type) {
        switch (type) {
            case OverlayType.none:
            default:
                powerOverlay.gameObject.SetActive(false);
                titleText.text = "None";
                break;
            case OverlayType.power:
                outlineImage.color = powerOverlayColors.enabledColor;
                titleBoxImage.color = powerOverlayColors.enabledColor;
                titleText.color = powerOverlayColors.enabledColor;
                powerOverlay.gameObject.SetActive(true);
                titleText.text = "Power";
                break;
            case OverlayType.cyber:
                outlineImage.color = cyberOverlayColors.enabledColor;
                titleBoxImage.color = cyberOverlayColors.enabledColor;
                titleText.color = cyberOverlayColors.enabledColor;
                powerOverlay.gameObject.SetActive(false);
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
