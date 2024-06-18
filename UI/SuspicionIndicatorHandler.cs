using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Easings;
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
    [Header("Alarm / disguise modifiers")]
    public RectTransform col2Mask;
    public GameObject alarmChevronObject;
    public GameObject disguiseChevronObject;
    public GameObject topChevronSpacer;
    public GameObject bottomChevronSpacer;
    public RectTransform alarmChevronRect;
    public RectTransform disguiseChevronRect;
    public RectTransform bottomSpacerRect;
    public RectTransform topSpacerRect;
    public VerticalLayoutGroup rightChevronVerticalLayoutGroup;
    public RectTransform alarmFillBarRectTransform;
    public float alarmChevronEaseTimer;
    public float disguiseChevronEaseTimer;
    public bool easeInAlarmChevron;
    public bool easeInDisguiseChevron;
    public bool easeOutAlarmChevron;
    public bool easeOutDisguiseChevron;
    public float alarmChevronEaseDuration = 1f;
    float targetLeftLineHeight;
    float targetRightLineHeight;
    bool doUpdate;
    public void Bind() {
        GameManager.OnSuspicionChange += HandleSuspicionChange;
        HandleSuspicionChange();
    }
    void OnDestroy() {
        GameManager.OnSuspicionChange -= HandleSuspicionChange;
    }
    void Start() {
        foreach (Transform child in statusRecordsContainer) {
            Destroy(child.gameObject);
        }
        for (int i = 0; i < 10; i++) {
            GameObject obj = GameObject.Instantiate(statusRecordPrefab);
            obj.transform.SetParent(statusRecordsContainer, worldPositionStays: false);
            obj.SetActive(false);
        }
    }

    public void HandleSuspicionChange() {
        doUpdate = true;
    }

    void LateUpdate() {
        if (GameManager.I.gameData.levelState?.template == null) return;
        if (doUpdate) {
            UpdateSensitivity();

            UpdateStatusFeed();

            UpdateIndicators();

            SetSummaryText();
        }
        doUpdate = false;
    }
    public void UpdateSensitivity() {
        switch (GameManager.I.GetCurrentSensitivity()) {
            case SensitivityLevel.publicProperty:
                sensitivityText.text = ">> public property <<";
                sensitivityText.color = green;
                break;
            case SensitivityLevel.semiprivateProperty:
                sensitivityText.text = ">> protected property <<";
                sensitivityText.color = yellow;
                break;
            case SensitivityLevel.privateProperty:
                sensitivityText.text = ">> private property <<";
                sensitivityText.color = yellow;
                break;
            case SensitivityLevel.restrictedProperty:
                sensitivityText.text = ">> restricted area <<";
                sensitivityText.color = red;
                break;
        }
    }
    public void UpdateStatusFeed() {
        List<string> activeRecords = new List<string>();

        foreach (Transform child in statusRecordsContainer) {
            SuspicionStatusRecordIndicatorHandler handler = child.GetComponent<SuspicionStatusRecordIndicatorHandler>();
            if (handler != null && handler.suspicionRecord != null)
                activeRecords.Add(handler.suspicionRecord.content);
        }

        List<SuspicionRecord> records = GameManager.I.suspicionRecords.Values.ToList();
        for (int i = 0; i < records.Count; i++) {
            SuspicionRecord record = records[i];
            Transform obj = statusRecordsContainer.GetChild(i);
            if (!obj.gameObject.activeInHierarchy)
                obj.gameObject.SetActive(true);
            SuspicionStatusRecordIndicatorHandler handler = obj.GetComponent<SuspicionStatusRecordIndicatorHandler>();
            handler.Configure(record, newRecord: !activeRecords.Contains(record.content));
        }
        for (int i = records.Count; i < statusRecordsContainer.childCount; i++) {
            Transform child = statusRecordsContainer.GetChild(i);
            if (child.gameObject.activeInHierarchy)
                child.gameObject.SetActive(false);
        }
    }

    void Update() {
        float leftHeight = Mathf.Lerp(leftLine.rect.height, targetLeftLineHeight, 0.1f);
        float rightHeight = Mathf.Lerp(rightLine.rect.height, targetRightLineHeight, 0.1f);
        SetLineHeight(leftLine, leftHeight);
        SetLineHeight(rightLine, rightHeight);

        if (easeInAlarmChevron) {
            float factor = 1f;
            alarmChevronEaseTimer += Time.unscaledDeltaTime;
            if (alarmChevronEaseTimer < alarmChevronEaseDuration) {
                factor = (float)PennerDoubleAnimation.QuartEaseOut(alarmChevronEaseTimer, 0f, 1f, alarmChevronEaseDuration);
            } else {
                easeInAlarmChevron = false;
            }
            Vector2 size = new Vector2(215f, factor * 33f);
            alarmChevronRect.sizeDelta = size;
            topSpacerRect.sizeDelta = size;
            col2Mask.anchoredPosition = new Vector2(0f, factor * -55f);
        }
        if (easeInDisguiseChevron) {
            float factor = 1f;
            disguiseChevronEaseTimer += Time.unscaledDeltaTime;
            if (disguiseChevronEaseTimer < alarmChevronEaseDuration) {
                factor = (float)PennerDoubleAnimation.QuartEaseOut(disguiseChevronEaseTimer, 0f, 1f, alarmChevronEaseDuration);
            } else {
                easeInDisguiseChevron = false;
            }
            rightChevronVerticalLayoutGroup.padding = new RectOffset(-30, 0, (int)(16f - 60f * factor), 50);
            disguiseChevronRect.sizeDelta = new Vector2(215f, factor * 33f);
            bottomSpacerRect.sizeDelta = new Vector2(215f, factor * 33f);
        }
        if (GameManager.I.gameData.levelState?.anyAlarmTerminalActivated() ?? false) {
            Rect fillBarRect = alarmFillBarRectTransform.rect;
            float factor = 150f * (GameManager.I.alarmCountdown() / 30f);
            alarmFillBarRectTransform.sizeDelta = new Vector2(factor, 1f);
        }
    }

    public void UpdateIndicators() {
        Suspiciousness netSuspicion = GameManager.I.GetTotalSuspicion();
        Reaction reaction = GameManager.I.GetSuspicionReaction(netSuspicion);

        switch (netSuspicion) {
            case Suspiciousness.normal:
                suspicionNormalChevron.color = activeColor;
                suspicionSuspiciousChevron.color = disabledColor;
                suspicionAggressiveChevron.color = disabledColor;
                SetLeftLineTargetHeight(78);

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
                SetLeftLineTargetHeight(194);

                SetNormalConnectionLineColor(disabledColor);
                SetSuspiciousConnectionLineColor(disabledColor);
                SetAggroConnectionLineColor(activeColor);

                suspicionNormalLight.color = lightGreenDisabled;
                suspicionSuspiciousLight.color = lightYellowDisabled;
                suspicionAggressiveLight.color = lightRedActive;
                break;
        }
        bool alarmActive = GameManager.I.gameData.levelState.anyAlarmTerminalActivated();
        bool disguiseActive = GameManager.I.gameData.levelState.delta.disguise;
        SensitivityLevel sensitivityLevel = GameManager.I.gameData.levelState.template.sensitivityLevel;
        switch (reaction) {
            case Reaction.ignore:
                reactionIgnoreChevron.color = activeColor;
                reactionInvestigateChevron.color = disabledColor;
                reactionAttackChevron.color = disabledColor;
                reactionIgnoreLight.color = lightGreenActive;
                reactionInvestigateLight.color = lightYellowDisabled;
                reactionAttackLight.color = lightRedDisabled;
                if (disguiseActive && !alarmActive && sensitivityLevel != SensitivityLevel.restrictedProperty) {
                    if (netSuspicion == Suspiciousness.suspicious) {
                        if (sensitivityLevel == SensitivityLevel.semiprivateProperty || sensitivityLevel == SensitivityLevel.publicProperty) {
                            reactionIgnoreChevron.color = disabledColor;
                            reactionIgnoreLight.color = lightGreenDisabled;
                        }
                    } else {
                        reactionIgnoreChevron.color = disabledColor;
                        reactionIgnoreLight.color = lightGreenDisabled;
                    }
                }
                break;
            case Reaction.investigate:
                reactionIgnoreChevron.color = disabledColor;
                reactionInvestigateChevron.color = activeColor;
                reactionAttackChevron.color = disabledColor;
                reactionIgnoreLight.color = lightGreenDisabled;
                reactionInvestigateLight.color = lightYellowActive;
                reactionAttackLight.color = lightRedDisabled;
                break;
            case Reaction.attack:
                reactionIgnoreChevron.color = disabledColor;
                reactionInvestigateChevron.color = disabledColor;
                reactionAttackChevron.color = activeColor;
                reactionIgnoreLight.color = lightGreenDisabled;
                reactionInvestigateLight.color = lightYellowDisabled;
                reactionAttackLight.color = lightRedActive;

                if (alarmActive && !disguiseActive) {
                    if (netSuspicion == Suspiciousness.normal) {

                    } else if (netSuspicion == Suspiciousness.suspicious && sensitivityLevel != SensitivityLevel.privateProperty) {
                        reactionAttackChevron.color = disabledColor;
                        reactionAttackLight.color = lightRedDisabled;
                    } else if (netSuspicion == Suspiciousness.aggressive) {
                        reactionAttackChevron.color = disabledColor;
                        reactionAttackLight.color = lightRedDisabled;
                    }
                }
                break;
        }
        switch (GameManager.I.GetSuspicionReaction(netSuspicion, applyModifiers: false)) {
            case Reaction.ignore:
                SetRightLineTargetHeight(145);
                break;
            case Reaction.investigate:
                SetRightLineTargetHeight(85);
                break;
            case Reaction.attack:
                SetRightLineTargetHeight(27);
                break;
        }

        if (alarmActive) {
            if (!alarmChevronObject.activeInHierarchy) {
                easeInAlarmChevron = true;
                alarmChevronEaseTimer = 0f;
            }
            alarmChevronObject.SetActive(true);
        } else {
            alarmChevronObject.SetActive(false);
            col2Mask.anchoredPosition = Vector2.zero;
        }

        if (disguiseActive) {
            if (!disguiseChevronObject.activeInHierarchy) {
                easeInDisguiseChevron = true;
                disguiseChevronEaseTimer = 0f;
            }
            disguiseChevronObject.SetActive(true);
        } else {
            disguiseChevronObject.SetActive(false);
            rightChevronVerticalLayoutGroup.padding = new RectOffset(-30, 0, 16, 50);
        }
    }

    public void SetSuspiciousConnectionLineColor(Color color) {
        // don't apply modifiers here
        Reaction reaction = GameManager.I.GetSuspicionReaction(Suspiciousness.suspicious, applyModifiers: false);
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
        // don't apply modifiers here

        Reaction reaction = GameManager.I.GetSuspicionReaction(Suspiciousness.normal, applyModifiers: false);
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
        // don't apply modifiers here

        Reaction reaction = GameManager.I.GetSuspicionReaction(Suspiciousness.aggressive, applyModifiers: false);
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
            Suspiciousness.suspicious => "appearance ... suspicious",
            Suspiciousness.aggressive => "appearance ... aggressive",
            _ => ""
        };
        summaryText2.text = reaction switch {
            Reaction.ignore => "status: innocent bystander",
            Reaction.investigate => "status: attracting attention",
            Reaction.attack => "status: attack on sight",
            _ => ""
        };
    }
}
