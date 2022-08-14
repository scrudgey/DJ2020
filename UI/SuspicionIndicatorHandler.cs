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
    public Image susAggroConnectionLine;

    [Header("Normal connectors")]
    public Image[] susNormalConnectionInvestigateLines;
    public Image[] susNormalConnectionIgnoreLines;
    public Image[] susNormalConnectionAttackLines;

    [Header("Suspicious connectors")]
    public Image[] susSuspiciousConnectionInvestigateLines;
    public Image[] susSuspiciousConnectionIgnoreLines;
    public Image[] susSuspiciousConnectionAttackLines;

    [Header("Aggro connectors")]
    public Image[] susAggroConnectionInvestigateLines;
    public Image[] susAggroConnectionIgnoreLines;
    public Image[] susAggroConnectionAttackLines;

    [Header("Summary")]
    public TextMeshProUGUI summaryText1;
    public TextMeshProUGUI summaryText2;

    float targetLeftLineHeight;
    float targetRightLineHeight;
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
        // List<string> activeRecords = GameManager.I.suspicionRecords.Keys.ToList();
        List<string> activeRecords = new List<string>();

        foreach (Transform child in statusRecordsContainer) {
            SuspicionStatusRecordIndicatorHandler handler = child.GetComponent<SuspicionStatusRecordIndicatorHandler>();
            if (handler != null && handler.suspicionRecord != null)
                activeRecords.Add(handler.suspicionRecord.content);
            Destroy(child.gameObject);
        }

        foreach (SuspicionRecord record in GameManager.I.suspicionRecords.Values) {
            GameObject obj = GameObject.Instantiate(statusRecordPrefab);
            obj.transform.SetParent(statusRecordsContainer, worldPositionStays: false);
            SuspicionStatusRecordIndicatorHandler handler = obj.GetComponent<SuspicionStatusRecordIndicatorHandler>();
            handler.Configure(record, newRecord: !activeRecords.Contains(record.content));
        }
    }

    void Update() {
        float leftHeight = Mathf.Lerp(leftLine.rect.height, targetLeftLineHeight, 0.1f);
        float rightHeight = Mathf.Lerp(rightLine.rect.height, targetRightLineHeight, 0.1f);
        SetLineHeight(leftLine, leftHeight);
        SetLineHeight(rightLine, rightHeight);
    }

    public void UpdateIndicators() {
        Suspiciousness netSuspicion = GameManager.I.GetTotalSuspicion();
        Reaction reaction = GameManager.I.GetSuspicionReaction(netSuspicion);

        switch (netSuspicion) {
            case Suspiciousness.normal:
                suspicionNormalChevron.color = activeColor;
                suspicionSuspiciousChevron.color = disabledColor;
                suspicionAggressiveChevron.color = disabledColor;
                SetLeftLineTargetHeight(194);

                SetNormalConnectionLineColor(activeColor);
                SetSuspiciousConnectionLineColor(disabledColor);
                SetAggroConnectionLineColor(disabledColor);

                suspicionNormalLight.color = lightGreenActive;
                suspicionSuspiciousLight.color = lightYellowDisabled;
                suspicionAggressiveLight.color = lightRedDisabled;
                break;
            case Suspiciousness.suspicious:
                suspicionNormalChevron.color = disabledColor;
                suspicionSuspiciousChevron.color = activeColor;
                suspicionAggressiveChevron.color = disabledColor;
                SetLeftLineTargetHeight(137);

                SetNormalConnectionLineColor(disabledColor);
                SetSuspiciousConnectionLineColor(activeColor);
                SetAggroConnectionLineColor(disabledColor);

                suspicionNormalLight.color = lightGreenDisabled;
                suspicionSuspiciousLight.color = lightYellowActive;
                suspicionAggressiveLight.color = lightRedDisabled;
                break;
            case Suspiciousness.aggressive:
                suspicionNormalChevron.color = disabledColor;
                suspicionSuspiciousChevron.color = disabledColor;
                suspicionAggressiveChevron.color = activeColor;
                SetLeftLineTargetHeight(78);

                SetNormalConnectionLineColor(disabledColor);
                SetSuspiciousConnectionLineColor(disabledColor);
                SetAggroConnectionLineColor(activeColor);

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
                SetRightLineTargetHeight(27);
                reactionIgnoreLight.color = lightGreenActive;
                reactionInvestigateLight.color = lightYellowDisabled;
                reactionAttackLight.color = lightRedDisabled;
                break;
            case Reaction.investigate:
                reactionIgnoreChevron.color = disabledColor;
                reactionInvestigateChevron.color = activeColor;
                reactionAttackChevron.color = disabledColor;
                SetRightLineTargetHeight(85);
                reactionIgnoreLight.color = lightGreenDisabled;
                reactionInvestigateLight.color = lightYellowActive;
                reactionAttackLight.color = lightRedDisabled;
                break;
            case Reaction.attack:
                reactionIgnoreChevron.color = disabledColor;
                reactionInvestigateChevron.color = disabledColor;
                reactionAttackChevron.color = activeColor;
                SetRightLineTargetHeight(145);
                reactionIgnoreLight.color = lightGreenDisabled;
                reactionInvestigateLight.color = lightYellowDisabled;
                reactionAttackLight.color = lightRedActive;
                break;
        }
    }

    public void SetSuspiciousConnectionLineColor(Color color) {
        Reaction reaction = GameManager.I.GetSuspicionReaction(Suspiciousness.suspicious);
        switch (reaction) {
            case Reaction.ignore:
                foreach (Image image in susSuspiciousConnectionIgnoreLines) {
                    image.color = color;
                    image.enabled = true;
                }
                foreach (Image image in susSuspiciousConnectionInvestigateLines) {
                    image.enabled = false;
                }
                foreach (Image image in susSuspiciousConnectionAttackLines) {
                    image.enabled = false;
                }
                break;
            case Reaction.investigate:
                foreach (Image image in susSuspiciousConnectionIgnoreLines) {
                    image.enabled = false;
                }
                foreach (Image image in susSuspiciousConnectionInvestigateLines) {
                    image.color = color;
                    image.enabled = true;
                }
                foreach (Image image in susSuspiciousConnectionAttackLines) {
                    image.enabled = false;
                }
                break;
            case Reaction.attack:
                foreach (Image image in susSuspiciousConnectionIgnoreLines) {
                    image.enabled = false;
                }
                foreach (Image image in susSuspiciousConnectionInvestigateLines) {
                    image.enabled = false;
                }
                foreach (Image image in susSuspiciousConnectionAttackLines) {
                    image.color = color;
                    image.enabled = true;
                }
                break;
        }
    }

    public void SetNormalConnectionLineColor(Color color) {
        Reaction reaction = GameManager.I.GetSuspicionReaction(Suspiciousness.normal);
        switch (reaction) {
            case Reaction.ignore:
                foreach (Image image in susNormalConnectionIgnoreLines) {
                    image.color = color;
                    image.enabled = true;
                }
                foreach (Image image in susNormalConnectionInvestigateLines) {
                    image.enabled = false;
                }
                foreach (Image image in susNormalConnectionAttackLines) {
                    image.enabled = false;
                }
                break;
            case Reaction.investigate:
                foreach (Image image in susNormalConnectionIgnoreLines) {
                    image.enabled = false;
                }
                foreach (Image image in susNormalConnectionInvestigateLines) {
                    image.color = color;
                    image.enabled = true;
                }
                foreach (Image image in susSuspiciousConnectionAttackLines) {
                    image.enabled = false;
                }
                break;
            case Reaction.attack:
                foreach (Image image in susNormalConnectionIgnoreLines) {
                    image.enabled = false;
                }
                foreach (Image image in susNormalConnectionInvestigateLines) {
                    image.enabled = false;
                }
                foreach (Image image in susNormalConnectionAttackLines) {
                    image.color = color;
                    image.enabled = true;
                }
                break;
        }
    }


    public void SetAggroConnectionLineColor(Color color) {
        Reaction reaction = GameManager.I.GetSuspicionReaction(Suspiciousness.aggressive);
        switch (reaction) {
            case Reaction.ignore:
                foreach (Image image in susAggroConnectionIgnoreLines) {
                    image.color = color;
                    image.enabled = true;
                }
                foreach (Image image in susAggroConnectionInvestigateLines) {
                    image.enabled = false;
                }
                foreach (Image image in susAggroConnectionAttackLines) {
                    image.enabled = false;
                }
                break;
            case Reaction.investigate:
                foreach (Image image in susAggroConnectionIgnoreLines) {
                    image.enabled = false;
                }
                foreach (Image image in susAggroConnectionInvestigateLines) {
                    image.color = color;
                    image.enabled = true;
                }
                foreach (Image image in susAggroConnectionAttackLines) {
                    image.enabled = false;
                }
                break;
            case Reaction.attack:
                foreach (Image image in susAggroConnectionIgnoreLines) {
                    image.enabled = false;
                }
                foreach (Image image in susAggroConnectionInvestigateLines) {
                    image.enabled = false;
                }
                foreach (Image image in susAggroConnectionAttackLines) {
                    image.color = color;
                    image.enabled = true;
                }
                break;
        }
    }

    void SetLineHeight(RectTransform rectTransform, float height) {
        rectTransform.sizeDelta = new Vector2(3f, height);
    }
    void SetLeftLineTargetHeight(float height) {
        targetLeftLineHeight = height;
    }
    void SetRightLineTargetHeight(float height) {
        targetRightLineHeight = height;
    }

    void SetSummaryText() {
        Suspiciousness suspiciousness = GameManager.I.GetTotalSuspicion();
        Reaction reaction = GameManager.I.GetSuspicionReaction(suspiciousness);
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
