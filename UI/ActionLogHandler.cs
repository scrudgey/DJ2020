using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
public class ActionLogHandler : MonoBehaviour, IBinder<Interactor> {
    public Interactor target { get; set; }

    public TextMeshProUGUI logText;
    public TextMeshProUGUI promptText;
    public InteractorTargetData data;
    Coroutine blitTextCoroutine;

    static readonly string prefix = ">";
    static readonly string dot = "|";


    void Start() {
        logText.text = "";
    }

    // TODO: figure out a better pattern for this sort of thing
    public void Bind(GameObject newTargetObject) {
        if (this.target != null) {
            target.OnValueChanged -= HandleValueChanged;
            target.OnActionDone -= HandleActionDone;
        }
        target = newTargetObject.GetComponentInChildren<Interactor>();
        if (target != null) {
            target.OnValueChanged += HandleValueChanged;
            target.OnActionDone += HandleActionDone;
            HandleValueChanged(target);
        }
    }
    public void HandleValueChanged(Interactor interactor) {
        InteractorTargetData newData = interactor.ActiveTarget();
        if (newData != data) {
            data = newData;
            DataChanged();
        }
    }
    void HandleActionDone(InteractorTargetData data) {
        logText.text += $"\n>{data.target.actionPrompt}\n{data.target.ResponseString()}";
        // TODO: trim old lines
    }
    void DataChanged() {
        promptText.text = ">";
        if (blitTextCoroutine != null) {
            StopCoroutine(blitTextCoroutine);
        }
        if (data == null) {
            return;
        }
        blitTextCoroutine = StartCoroutine(BlitCalloutText(data.target.actionPrompt));
    }
    public IEnumerator BlitCalloutText(string actionText) {
        float blitInterval = 0.01f;
        float timer = 0f;
        int index = 1;
        string targetText = $"{prefix}{actionText}{dot}";
        while (promptText.text != targetText) {
            while (timer < blitInterval) {
                timer += Time.deltaTime;
                yield return null;
            }
            timer -= blitInterval;
            index += 1;
            promptText.text = targetText.Substring(0, index);
        }
        timer = 0f;
        blitInterval = 0.1f;
        while (true) {
            while (timer < blitInterval) {
                timer += Time.deltaTime;
                yield return null;
            }
            timer -= blitInterval;
            if (promptText.text == $"{prefix}{actionText}{dot}") {
                promptText.text = $"{prefix}{actionText}";
            } else {
                promptText.text = $"{prefix}{actionText}{dot}";
            }
            yield return null;
        }
    }
}
