using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Easings;
using TMPro;
using UnityEngine;
public class SkillCheckDialogue : MonoBehaviour {
    public class SkillCheckResult {
        public enum ResultType { success, fail }
        public ResultType type;
        public SkillCheckInput input;
    }
    public struct SkillCheckInput {
        public string checkType;
        public string successResponse;
        public string failResponse;
        public string suspicion;
        public float threshold;
    }
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI thresholdText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI resultText;
    public Transform barsContainer;
    public Color red;
    public Color blue;
    public Color green;

    public Color disabled;
    public Action<SkillCheckResult> resultCallback;
    Coroutine descriptionPulseCoroutine;
    public RectTransform arrowIndicator;
    public RectTransform arrowIndicatorParent;
    public AudioSource audioSource;
    public AudioClip[] beepSound;
    public AudioClip[] initializeSound;
    public AudioClip[] lockInSound;
    public AudioClip[] successSound;
    public AudioClip[] failSound;
    public void Initialize(Action<SkillCheckResult> resultCallback, SkillCheckInput input) {
        this.resultCallback = resultCallback;
        resultText.enabled = false;
        SetBarLevel(0);
        StartCoroutine(ProcessSkillCheck(input));
    }

    IEnumerator ProcessSkillCheck(SkillCheckInput input) {
        // TODO: set threshold based on state / input
        float threshold = 25f;
        float roll = UnityEngine.Random.Range(0f, 100f);
        // float roll = 20f;
        Debug.Log(roll);

        yield return StartUpIndicators(threshold, input);

        float timer = 0f;
        float totalInterval = 2f;

        float blinkInterval = UnityEngine.Random.Range(0.02f, 0.05f);
        float blinkTimer = 0f;
        float noise = 0f;
        while (timer < totalInterval) {
            timer += Time.unscaledDeltaTime;
            blinkTimer += Time.unscaledDeltaTime;

            if (blinkTimer > blinkInterval) {
                noise = UnityEngine.Random.Range(-10f, 10f);
                blinkTimer -= blinkInterval;
                blinkInterval = UnityEngine.Random.Range(0.02f, 0.05f) + (timer / 3f);
                Toolbox.RandomizeOneShot(audioSource, beepSound);
            }
            float rollQuantity = (float)PennerDoubleAnimation.CircEaseOut(timer, 0f, roll + noise, totalInterval);
            rollQuantity = Math.Max(0f, rollQuantity);
            rollQuantity = Math.Min(100f, rollQuantity);
            int rollIndex = (int)(rollQuantity / 10f);
            SetBarLevel(rollIndex);
            yield return null;
        }
        SetBarLevel((int)(roll / 10f));
        Debug.Log($"skill check: {roll} / {threshold}");

        // TODO: set response
        SkillCheckResult result = new SkillCheckResult {
            type = roll > threshold ? SkillCheckResult.ResultType.success : SkillCheckResult.ResultType.fail,
            input = input,
        };
        thresholdText.color = disabled;
        switch (result.type) {
            case SkillCheckResult.ResultType.success:
                Toolbox.RandomizeOneShot(audioSource, successSound, randomPitchWidth: 0.05f);
                resultText.text = "[success]";
                resultText.color = green;
                descriptionText.text = $"{input.checkType} check success!";
                StartCoroutine(BlinkEmphasis(resultText));
                break;
            case SkillCheckResult.ResultType.fail:
                Toolbox.RandomizeOneShot(audioSource, failSound, randomPitchWidth: 0.05f);

                resultText.text = "[fail]";
                resultText.color = red;
                descriptionText.text = $"{input.checkType} check failed!";
                StartCoroutine(BlinkEmphasis(resultText));
                break;
        }
        StopCoroutine(descriptionPulseCoroutine);
        yield return new WaitForSecondsRealtime(2f);
        resultCallback(result);
        yield return null;
    }
    IEnumerator StartUpIndicators(float targetThreshold, SkillCheckInput input) {
        descriptionText.text = "";
        titleText.text = $"- {input.checkType} check -";
        thresholdText.text = "0%";
        thresholdText.enabled = true;
        thresholdText.color = Color.white;
        float timer = 0f;
        float interval = 1f;
        float threshold = 0f;

        // don't move colors yet
        // arrow starts from wrong position
        // change lerp mode

        yield return new WaitForSecondsRealtime(0.2f);
        float xPosition = arrowIndicator.anchoredPosition.x;
        Toolbox.RandomizeOneShot(audioSource, initializeSound, randomPitchWidth: 0f);
        while (timer < interval) {
            timer += Time.unscaledDeltaTime;

            threshold = (float)PennerDoubleAnimation.CircEaseOut(timer, 0f, targetThreshold, interval);

            float height = arrowIndicatorParent.rect.height * threshold / 100f;

            arrowIndicator.anchoredPosition = new Vector2(xPosition, height);
            thresholdText.text = $"{(int)threshold}%";
            yield return null;
        }
        Toolbox.RandomizeOneShot(audioSource, lockInSound, randomPitchWidth: 0f);

        yield return BlinkEmphasis(thresholdText, pulses: 6);
        yield return new WaitForSecondsRealtime(0.2f);
        if (descriptionPulseCoroutine == null) {
            descriptionPulseCoroutine = StartCoroutine(PulseDescriptionText($"{input.checkType} check in progress", descriptionText));
        }
    }

    IEnumerator PulseDescriptionText(string baseText, TextMeshProUGUI text) {
        text.enabled = true;
        float timer = 0f;
        int index = 0;
        float interval = 0.2f;
        while (true) {
            timer += Time.unscaledDeltaTime;
            if (timer > interval) {
                index += 1;
                timer -= interval;
            }
            if (index > 3) {
                index = 0;
            }
            text.text = baseText + new string('.', index);
            yield return null;
        }
    }

    IEnumerator BlinkEmphasis(MonoBehaviour component, int pulses = 7) {
        float timer = 0f;
        int cycles = 0;
        while (cycles < pulses) {
            timer += Time.unscaledDeltaTime;
            if (timer > 0.1f) {
                timer -= 0.05f;
                cycles += 1;
                component.enabled = !component.enabled;
            }
            yield return null;
        }
    }

    void SetBarLevel(int value) {
        int i = 0;
        foreach (Transform child in barsContainer.Cast<Transform>().OrderBy(t => t.name).Reverse()) {
            if (child == null)
                continue;
            TextMeshProUGUI text = child.GetComponent<TextMeshProUGUI>();
            if (text == null)
                continue;
            text.color = i < value ? red : blue;
            i += 1;
        }
    }
}
