using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Easings;
using Nimrod;
using TMPro;
using UnityEngine;
public class SpeechTextController : MonoBehaviour {
    static public float PHRASE_DELAY = 3f;
    static public Dictionary<string, float> phraseTimes;
    public bool scaleWithPlayerDistance;
    public TextMeshProUGUI textMesh;
    public List<RectTransform> childRects;
    public RectTransform canvasRect;
    public Transform followTransform;
    public Camera cam;
    public List<string> grammarFiles;
    public Sprite portrait;

    Grammar grammar;
    float visibilityTimer;
    float haltSpeechTimeout;
    readonly static float FALLOFF_DISTANCE = 10f;
    Vector3 offset = new Vector3(0f, 2f, 0f);
    void Start() {
        grammar = new Grammar();
        if (grammarFiles.Count > 0) {
            foreach (string filename in grammarFiles) {
                grammar.Load(filename);
            }
        } else {
            grammar.Load("speech_default");
        }
        cam = Camera.main;
        HideText();
        visibilityTimer = 0f;
        SetRectPositions();
    }
    void OnDisable() {
        HideText();
    }
    void FixedUpdate() {
        if (haltSpeechTimeout > 0) {
            haltSpeechTimeout -= Time.fixedDeltaTime;
            HideText();
            return;
        }
        if (visibilityTimer > 0) {
            SetRectPositions();
            visibilityTimer -= Time.fixedDeltaTime;
            if (visibilityTimer <= 0) {
                HideText();
            }
        }
        if (scaleWithPlayerDistance && IsSpeaking()) {
            Vector3 displacement = followTransform.position - GameManager.I.playerPosition;
            float distance = displacement.magnitude;
            if (distance > FALLOFF_DISTANCE) {
                textMesh.transform.localScale = Vector3.zero;
            } else {
                textMesh.transform.localScale = new Vector3(1f, 1.2f, 1f) * (float)PennerDoubleAnimation.ExpoEaseOut(distance, 2f, -2f, FALLOFF_DISTANCE);
            }
        }
    }
    public void HaltSpeechForTime(float timeout) {
        HideText();
    }

    public void Say(string phrase, string color) {
        Say($"<color={color}>{phrase}</color>");
    }
    public void Say(string phrase) {
        textMesh.text = phrase;
        visibilityTimer = 5f;
        ShowText();
    }
    void SetRectPositions() {
        childRects.ForEach((rect) => rect.position = cam.WorldToScreenPoint(followTransform.position + offset));
    }

    void ShowText() {
        textMesh.enabled = true;
        SetRectPositions();
    }
    public void HideText() {
        textMesh.enabled = false;
    }
    public bool IsSpeaking() {
        return textMesh.enabled;
    }
    public void SayGrammar(string key) {
        if (CheckPhraseTimes(key)) {
            string phrase = grammar.Parse($"{{{key}}}");
            Say(phrase);
        }
    }
    public void SayGrammar(string key, string color) {
        if (CheckPhraseTimes(key)) {
            string phrase = grammar.Parse($"{{{key}}}");
            Say(phrase, color);
        }
    }

    public void SayAttack() {
        SayGrammar("attack", "#ff4757");
    }
    public void SaySpotted() {
        SayGrammar("spotted");
    }
    public void SayFreeze() {
        SayGrammar("spotted", "#ffa502");
    }
    public void SayHoldIt() {
        SayGrammar("holdit", "#ffa502");
    }
    public void SayWhatWasThat() {
        SayGrammar("what-was-that");
    }
    public void SayGuessItWasNothing() {
        SayGrammar("guess-nothing");
    }
    public void SayPageGuard() {
        SayGrammar("page-guard", "#ff4757");
    }


    bool CheckPhraseTimes(string phrase) {
        if (phraseTimes == null) {
            phraseTimes = new Dictionary<string, float>();
        }
        float timeNow = Time.timeSinceLevelLoad;
        if (phraseTimes.ContainsKey(phrase)) {
            float lastSaidTime = phraseTimes[phrase];
            if (timeNow - lastSaidTime > PHRASE_DELAY) {
                phraseTimes[phrase] = timeNow;
                return true;
            } else return false;
        } else {
            phraseTimes[phrase] = timeNow;
            return true;
        }
    }
}
