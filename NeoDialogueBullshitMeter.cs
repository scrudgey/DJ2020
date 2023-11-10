using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public TextMeshProUGUI dangerText;
    public TextMeshProUGUI bullshitThresholdText;
    public RectTransform thresholdBar;
    public RectTransform thresholdBarContainer;
    public Transform statusContainer;
    public GameObject statusPrefab;
    public Color deactivatedColor;
    public Color green;
    public Color orange;
    public Color red;
    Coroutine dangerRoutine;
    int targetBullshit;
    float currentBullshit;
    public int bullshitThreshold;
    float currentThreshold;
    int randomOffset;
    void Awake() {
        dangerText.enabled = false;
    }
    public void Initialize(DialogueInput input) {
        UpdateBars(0);
        UpdateThresholdPosition(0);
    }
    void InitializeStatusContainer(Dictionary<string, int> statusEffects) {
        foreach (Transform child in statusContainer) {
            Destroy(child.gameObject);
        }
        CreateStatusElement($"base value", 70, plain: true);
        foreach (KeyValuePair<string, int> entry in statusEffects) {
            CreateStatusElement(entry.Key, entry.Value);
        }
    }
    public IEnumerator SetBullshitThreshold(Dictionary<string, int> statusEffects) {
        InitializeStatusContainer(statusEffects);
        bullshitThreshold = 70 + statusEffects.Values.Sum();
        return moveThresholdBar(bullshitThreshold);
    }
    public void CreateStatusElement(string content, int alarmCount, bool plain = false) {
        GameObject statusObj = GameObject.Instantiate(statusPrefab);
        statusObj.transform.SetParent(statusContainer, false);
        DialogueStatusEntry status = statusObj.GetComponent<DialogueStatusEntry>();
        status.InitializeNumeric(alarmCount, content, plain: plain);
    }

    public IEnumerator SetTargetBullshit(int amount, Func<IEnumerator> doubterPulseFunc) {
        yield return null;
        Coroutine doubterRoutine = null;
        if (amount > bullshitThreshold) {
            Debug.Log($"starting danger routine: {amount} {bullshitThreshold}");
            if (dangerRoutine == null)
                dangerRoutine = StartCoroutine(PulseDoubterColor());
        } else {
            if (dangerRoutine != null) {
                StopCoroutine(dangerRoutine);
                dangerRoutine = null;
            }
        }

        if (amount > targetBullshit) {
            Toolbox.RandomizeOneShot(audioSource, increaseSound);
            doubterRoutine = StartCoroutine(doubterPulseFunc.Invoke());
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
        // IEnumerator stopper = Toolbox.CoroutineFunc(() => {
        //     if (doubterRoutine != null)

        // })
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
        float rand = UnityEngine.Random.Range(0f, 1f);
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



    public IEnumerator moveThresholdBar(int targetThreshold) {
        yield return null;
        Toolbox.RandomizeOneShot(audioSource, thresholdSound);
        IEnumerator mover = Toolbox.Ease(null, 0.5f, currentThreshold, targetThreshold, PennerDoubleAnimation.SineEaseOut, (amount) => {
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

    IEnumerator PulseDoubterColor() {
        dangerText.enabled = true;
        float timer = 0f;
        Color color = red;
        int pulses = 0;
        while (targetBullshit > bullshitThreshold) {
            timer += Time.unscaledDeltaTime;
            float factor = (float)PennerDoubleAnimation.CircEaseIn(timer, 1f, -1f, 1f);
            dangerText.color = new Color(red.r, red.g, red.b, factor);
            if (timer > 1f) {
                pulses += 1;
                timer -= 1f;
            }
            yield return null;
        }
        Debug.Log($"stopping danger routine: {targetBullshit} {bullshitThreshold}");
        dangerText.color = red;
        dangerText.enabled = false;
        dangerRoutine = null;
    }

    void OnDestroy() {
        if (dangerRoutine != null)
            StopCoroutine(dangerRoutine);
    }
}
