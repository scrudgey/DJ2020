using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class SuspicionIndicatorHandler : MonoBehaviour {
    [Header("World Condition")]
    public TextMeshProUGUI sensitivityText;
    public Color green;
    public Color yellow;
    public Color red;
    [Header("Status records")]
    public Transform statusRecordsContainer;
    public GameObject statusRecordPrefab;
    [Header("Reaction indicators")]
    public Color lightGreenActive;
    public Color lightYellowActive;
    public Color lightRedActive;
    public Color lightGreenDisabled;
    public Color lightYellowDisabled;
    public Color lightRedDisabled;
    public Image suspicionNormalChevron;
    public Image suspicionSuspiciousChevron;
    public Image suspicionAggressiveChevron;
    public Image suspicionNormalLight;
    public Image suspicionSuspiciousLight;
    public Image suspicionAggressiveLight;
    public Image reactionIgnoreChevron;
    public Image reactionInvestigateChevron;
    public Image reactionAttackChevron;

    public Image reactionIgnoreLight;
    public Image reactionInvestigateLight;
    public Image reactionAttackLight;

    public RectTransform leftLine;
    public RectTransform rightLine;
    public Color activeColor;
    public Color disabledColor;
    public Image susNormalConnectionLine;
    public Image susSuspiciousConnectionLine;
    public Image susAggroConnectionLine;
    [Header("Summary")]
    public TextMeshProUGUI summaryText1;
    public TextMeshProUGUI summaryText2;
    public void Bind() {
        GameManager.OnSuspicionChange += HandleSuspicionChange;
    }
    void OnDestroy() {
        GameManager.OnSuspicionChange -= HandleSuspicionChange;
    }

    public void HandleSuspicionChange() {
        UpdateSensitivity();

        UpdateStatusFeed();

        UpdateIndicators();

        SetSummaryText();
    }

    public void UpdateSensitivity() {
        switch (GameManager.I.GetCurrentSensitivity()) {
            case SensitivityLevel.publicProperty:
                sensitivityText.text = ">> public <<";
                sensitivityText.color = green;
                break;
            case SensitivityLevel.semiprivateProperty:
                sensitivityText.text = ">> protected <<";
                sensitivityText.color = yellow;
                break;
            case SensitivityLevel.privateProperty:
                sensitivityText.text = ">> private <<";
                sensitivityText.color = yellow;
                break;
            case SensitivityLevel.restrictedProperty:
                sensitivityText.text = ">> restricted <<";
                sensitivityText.color = red;
                break;
        }
    }
    public void UpdateStatusFeed() {
        List<string> activeRecords = GameManager.I.suspicionRecords.Keys.ToList();

        foreach (Transform child in statusRecordsContainer) {
            Destroy(child.gameObject);
        }

        foreach (SuspicionRecord record in GameManager.I.suspicionRecords.Values) {
            GameObject obj = GameObject.Instantiate(statusRecordPrefab);
            obj.transform.SetParent(statusRecordsContainer, worldPositionStays: false);
            SuspicionStatusRecordIndicatorHandler handler = obj.GetComponent<SuspicionStatusRecordIndicatorHandler>();
            handler.Configure(record);
        }
    }

    public void UpdateIndicators() {
        Suspiciousness netSuspicion = GameManager.I.GetTotalSuspicion();
        Reaction reaction = GameManager.I.GetSuspicionReaction();

        switch (netSuspicion) {
            case Suspiciousness.normal:
                suspicionNormalChevron.color = activeColor;
                suspicionSuspiciousChevron.color = disabledColor;
                suspicionAggressiveChevron.color = disabledColor;
                SetLineHeight(leftLine, 31);
                susNormalConnectionLine.color = activeColor;
                susSuspiciousConnectionLine.color = disabledColor;
                susAggroConnectionLine.color = disabledColor;

                suspicionNormalLight.color = lightGreenActive;
                suspicionSuspiciousLight.color = lightYellowDisabled;
                suspicionAggressiveLight.color = lightRedDisabled;
                break;
            case Suspiciousness.suspicious:
                suspicionNormalChevron.color = disabledColor;
                suspicionSuspiciousChevron.color = activeColor;
                suspicionAggressiveChevron.color = disabledColor;
                SetLineHeight(leftLine, 87);
                susNormalConnectionLine.color = disabledColor;
                susSuspiciousConnectionLine.color = activeColor;
                susAggroConnectionLine.color = disabledColor;

                suspicionNormalLight.color = lightGreenDisabled;
                suspicionSuspiciousLight.color = lightYellowActive;
                suspicionAggressiveLight.color = lightRedDisabled;
                break;
            case Suspiciousness.aggressive:
                suspicionNormalChevron.color = disabledColor;
                suspicionSuspiciousChevron.color = disabledColor;
                suspicionAggressiveChevron.color = activeColor;
                SetLineHeight(leftLine, 144);
                susNormalConnectionLine.color = disabledColor;
                susSuspiciousConnectionLine.color = disabledColor;
                susAggroConnectionLine.color = activeColor;

                suspicionNormalLight.color = lightGreenDisabled;
                suspicionSuspiciousLight.color = lightYellowDisabled;
                suspicionAggressiveLight.color = lightRedActive;
                break;
        }

        switch (reaction) {
            case Reaction.ignore:
                reactionIgnoreChevron.color = activeColor;
                reactionInvestigateChevron.color = disabledColor;
                reactionAttackChevron.color = disabledColor;
                SetLineHeight(rightLine, 202);

                reactionIgnoreLight.color = lightGreenActive;
                reactionInvestigateLight.color = lightYellowDisabled;
                reactionAttackLight.color = lightRedDisabled;
                break;
            case Reaction.investigate:
                reactionIgnoreChevron.color = disabledColor;
                reactionInvestigateChevron.color = activeColor;
                reactionAttackChevron.color = disabledColor;
                SetLineHeight(rightLine, 143);

                reactionIgnoreLight.color = lightGreenDisabled;
                reactionInvestigateLight.color = lightYellowActive;
                reactionAttackLight.color = lightRedDisabled;
                break;
            case Reaction.attack:
                reactionIgnoreChevron.color = disabledColor;
                reactionInvestigateChevron.color = disabledColor;
                reactionAttackChevron.color = activeColor;
                SetLineHeight(rightLine, 85);

                reactionIgnoreLight.color = lightGreenDisabled;
                reactionInvestigateLight.color = lightYellowDisabled;
                reactionAttackLight.color = lightRedActive;
                break;
        }
    }

    void SetLineHeight(RectTransform rectTransform, float height) {
        rectTransform.sizeDelta = new Vector2(3f, height);
    }

    void SetSummaryText() {
        Suspiciousness suspiciousness = GameManager.I.GetTotalSuspicion();
        Reaction reaction = GameManager.I.GetSuspicionReaction();
        summaryText1.text = suspiciousness switch {
            Suspiciousness.normal => "appearance ... OK <i>!!<.i>",
            Suspiciousness.suspicious => "appearance ... shady",
            Suspiciousness.aggressive => "appearance ... enemy",
            _ => ""
        };
        summaryText2.text = reaction switch {
            Reaction.ignore => "status: innocent bystander",
            Reaction.investigate => "status: attracting attention",
            Reaction.attack => "status: enemy",
            _ => ""
        };
    }
}
