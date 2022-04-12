using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SuspicionUIHandler : MonoBehaviour {
    public SuspicionAppeaeranceUIHandler suspicionAppeaeranceUIHandler;
    public SuspicionAudioUIHandler suspicionAudioUIHandler;
    public SuspicionIconUIHandler suspicionIconUIHandler;
    public SuspicionVisualUIHandler suspicionVisualUIHandler;
    public TextMeshProUGUI environmentalText;
    public TextMeshProUGUI titleText;
    public Image background;
    public Color normalColor;
    public Color warnColor;
    public Color alertColor;
    public void Bind(GameObject target) {
        suspicionAppeaeranceUIHandler.Bind(this);
        suspicionAudioUIHandler.Bind(this);
        suspicionIconUIHandler.Bind(this);
        suspicionVisualUIHandler.Bind(this);
    }
    public void OnValueChanged() {
        Suspiciousness value = Suspiciousness.normal;
        List<Suspiciousness> indicators = new List<Suspiciousness>(){
            suspicionAppeaeranceUIHandler.appearance,
            suspicionAudioUIHandler.suspiciousness,
            suspicionVisualUIHandler.appearance
        };
        foreach (Suspiciousness indicator in indicators) {
            switch (value) {
                case Suspiciousness.normal:
                    if (indicator == Suspiciousness.suspicious || indicator == Suspiciousness.aggressive) {
                        value = indicator;
                    }
                    break;
                case Suspiciousness.suspicious:
                    if (indicator == Suspiciousness.aggressive) {
                        value = indicator;
                    }
                    break;
                case Suspiciousness.aggressive:
                    break;
            }
        }
        suspicionIconUIHandler.UpdateImage(value);
        switch (value) {
            default:
            case Suspiciousness.normal:
                titleText.text = "NORMAL";
                background.color = normalColor;
                titleText.color = normalColor;
                break;
            case Suspiciousness.suspicious:
                titleText.text = "SHADY";
                background.color = warnColor;
                titleText.color = warnColor;
                break;
            case Suspiciousness.aggressive:
                titleText.text = "AGGRO";
                background.color = alertColor;
                titleText.color = alertColor;
                break;
        }
        switch (GameManager.I.gameData.levelData.sensitivityLevel) {
            default:
            case SensitivityLevel.publicProperty:
                environmentalText.text = "PUBLIC";
                environmentalText.color = normalColor;
                break;
            case SensitivityLevel.semiprivateProperty:
                environmentalText.text = "SEMIPRIV";
                environmentalText.color = warnColor;
                break;
            case SensitivityLevel.privateProperty:
                environmentalText.text = "TRESPASSING";
                environmentalText.color = warnColor;
                break;
            case SensitivityLevel.restrictedProperty:
                environmentalText.text = "TRESPASSING";
                environmentalText.color = alertColor;
                break;
        }
    }
}
