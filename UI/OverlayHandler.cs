using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
public class OverlayHandler : MonoBehaviour {
    public GameObject outline;
    public TextMeshProUGUI titleText;
    public PowerOverlay powerOverlay;
    private Camera _cam;
    public Camera cam {
        get { return _cam; }
        set {
            _cam = value;
            powerOverlay.cam = value;
        }
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
                powerOverlay.gameObject.SetActive(true);
                titleText.text = "Power";
                break;
            case OverlayType.cyber:
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
