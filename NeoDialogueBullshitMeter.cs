using System.Collections;
using System.Collections.Generic;
using Easings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class NeoDialogueBullshitMeter : MonoBehaviour {
    readonly static int MAXIMUM_BULLSHIT = 100;
    public AudioSource audioSource;
    public AudioClip[] increaseSound;
    public AudioClip[] decreaseSound;
    public AudioClip[] thresholdSound;
    public List<Image> bars;
    public TextMeshProUGUI bullshitCounterText;
    public TextMeshProUGUI bullshitThresholdText;
    public RectTransform thresholdBar;
    public RectTransform thresholdBarContainer;
    public Transform statusContainer;
    public GameObject statusPrefab;
    public Color deactivatedColor;
    public Color green;
    public Color orange;
    public Color red;

    int targetBullshit;
    float currentBullshit;
    public int bullshitThreshold;
    float currentThreshold;
    int randomOffset;

    public void Initialize(DialogueInput input) {
        UpdateBars(0);
        UpdateThresholdPosition(0);
        InitializeStatusContainer(input);
    }
    void InitializeStatusContainer(DialogueInput input) {
        foreach (Transform child in statusContainer) {
            Destroy(child.gameObject);
        }
        if (input.alarmActive) {
            CreateStatusElement("alarm is active", 1);
        }
        switch (input.npcCharacter.alertness) {
            case Alertness.normal:
                CreateStatusElement("normal posture", 0);
                break;
            case Alertness.alert:
                CreateStatusElement("on alert", 2);
                break;
            case Alertness.distracted:
                CreateStatusElement("distracted", -1);
                break;
        }
        switch (input.levelState.template.sensitivityLevel) {
            case SensitivityLevel.publicProperty:
                CreateStatusElement("on public property", -1);
                break;
            case SensitivityLevel.semiprivateProperty:
            case SensitivityLevel.privateProperty:
                CreateStatusElement("on private property", 1);
                break;
            case SensitivityLevel.restrictedProperty:
                CreateStatusElement("in restricted area", 2);
                break;
        }
    }
    public void CreateStatusElement(string content, int alarmCount) {
        GameObject statusObj = GameObject.Instantiate(statusPrefab);
        statusObj.transform.SetParent(statusContainer, false);
        DialogueStatusEntry status = statusObj.GetComponent<DialogueStatusEntry>();
        status.InitializeNumeric(alarmCount, content);
    }

    public IEnumerator SetTargetBullshit(int amount) {
        yield return null;
        if (amount > targetBullshit) {
            Toolbox.RandomizeOneShot(audioSource, increaseSound);
        } else if (amount < targetBullshit) {
            Toolbox.RandomizeOneShot(audioSource, decreaseSound);
        }
        targetBullshit = amount;
        // Mathf.Abs(currentBullshit - targetBullshit) / 50f
        float duration = currentBullshit == targetBullshit ? 0f : 1f;
        IEnumerator meterMove = Toolbox.Ease(null, duration, currentBullshit, targetBullshit, PennerDoubleAnimation.Linear, (amount) => {
            UpdateBars(amount);
            currentBullshit = amount;
        }, unscaledTime: true);
        IEnumerator blinker = Toolbox.BlinkEmphasis(bullshitCounterText);
        yield return Toolbox.ChainCoroutines(meterMove, blinker);
    }

    void Update() {
        // float randomOffset = Random.Range(-40f, 40f);
        currentBullshit = Mathf.Lerp(currentBullshit, targetBullshit, 0.01f);
        currentBullshit = Mathf.Max(0f, currentBullshit);
        currentBullshit = Mathf.Min(MAXIMUM_BULLSHIT, currentBullshit);
        UpdateRandomOffset();
        UpdateBars(currentBullshit);
    }

    void UpdateRandomOffset() {
        float rand = Random.Range(0f, 1f);
        if (rand < 0.02f) {
            if (randomOffset < 1)
                randomOffset += 1;
        } else if (rand > 0.98f) {
            if (randomOffset > -1)
                randomOffset -= 1;
        }
    }

    void UpdateBars(float amount) {
        // change colors of bars
        int numberActiveBars = (int)(bars.Count * amount / (float)MAXIMUM_BULLSHIT);
        bullshitCounterText.text = $"{(int)amount}";
        for (int i = 0; i < bars.Count; i++) {
            int j = (bars.Count - 1) - i;
            if (i <= numberActiveBars + randomOffset) {
                Color targetColor = i switch {
                    < 9 => green,
                    < 13 => orange,
                    _ => red
                };
                bars[j].color = targetColor;
            } else {
                bars[j].color = deactivatedColor;
            }
        }
    }



    public IEnumerator SetBullshitThreshold(int threshold) {
        // bullshitThresholdText.text = $"{threshold}";
        bullshitThreshold = threshold;
        return moveThresholdBar(bullshitThreshold);
    }
    public IEnumerator moveThresholdBar(int targetThreshold) {
        yield return null;
        Toolbox.RandomizeOneShot(audioSource, thresholdSound);
        IEnumerator mover = Toolbox.Ease(null, 1f, currentThreshold, targetThreshold, PennerDoubleAnimation.ExpoEaseOut, (amount) => {
            currentThreshold = amount;
            UpdateThresholdPosition((int)amount);
        }, unscaledTime: true);
        IEnumerator blinker = Toolbox.BlinkEmphasis(bullshitThresholdText);
        yield return Toolbox.ChainCoroutines(mover, blinker);
    }

    void UpdateThresholdPosition(int threshold) {
        float fraction = (float)threshold / (float)MAXIMUM_BULLSHIT;
        float bottomOffset = 73f;
        float targetHeight = fraction * (thresholdBarContainer.rect.height - bottomOffset) + bottomOffset;
        thresholdBar.anchoredPosition = new Vector2(0f, targetHeight);
        bullshitThresholdText.text = $"{(int)(threshold)}";
    }
}
