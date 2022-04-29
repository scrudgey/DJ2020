using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    void Start() {
        suspicionAudioUIHandler.image.color = normalColor;
    }
    public void Bind() {
        GameManager.OnSuspicionDataChange += OnValueChanged;
        SuspicionData data = GameManager.I.GetSuspicionData();
        OnValueChanged(data);
    }
    void OnDestroy() {
        GameManager.OnSuspicionDataChange -= OnValueChanged;
    }
    public void OnValueChanged(SuspicionData data) {
        suspicionAppeaeranceUIHandler.HandleValueChange(data, this);
        suspicionVisualUIHandler.HandleValueChange(data, this);
        suspicionIconUIHandler.HandleValueChange(data, this);
        switch (data.netValue()) {
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
        switch (data.levelSensitivity) {
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
