using System.Collections;
using System.Collections.Generic;
using Easings;
using TMPro;
using UnityEngine;
public class AlarmIndicatorHandler : MonoBehaviour {
    enum State { none, easeIn, running, easeOut }
    State state;
    public TextMeshProUGUI[] timerTexts;
    public RectTransform boxRectTransform;
    float easingTimer;
    readonly static float easingDuration = 0.5f;
    void Update() {
        if (GameManager.I.alarmCountdown() > 0) {
            if (state != State.easeIn && state != State.running) {
                state = State.easeIn;
                easingTimer = 0f;
            }
            SetText();
        } else {
            if (state != State.easeOut && state != State.none) {
                state = State.easeOut;
                easingTimer = 0f;
            }
        }

        if (state == State.easeIn || state == State.easeOut) {
            easingTimer += Time.unscaledDeltaTime;
            if (easingTimer >= easingDuration) {
                if (state == State.easeIn) state = State.running;
                if (state == State.easeOut) state = State.none;
            }
        }

        float scale = state switch {
            State.easeIn => (float)PennerDoubleAnimation.ExpoEaseIn(easingTimer, 0f, 1f, easingDuration),
            State.easeOut => (float)PennerDoubleAnimation.ExpoEaseOut(easingTimer, 1f, -1f, easingDuration),
            State.none => 0f,
            State.running => 1f,
            _ => 1f
        };

        boxRectTransform.localScale = new Vector3(1f, scale, 1f);
    }

    void SetText() {
        foreach (TextMeshProUGUI timerText in timerTexts) {
            timerText.text = GameManager.I.alarmCountdown().ToString("00.00");
        }
    }
}
