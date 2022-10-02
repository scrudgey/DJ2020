using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
public class ActionLogHandler : IBinder<Interactor> {
    public Transform logTextHolder;
    public GameObject logTextEntryPrefab;
    public TextMeshProUGUI promptText;
    public InteractorTargetData data;
    Coroutine blitTextCoroutine;
    public AudioSource audioSource;
    static readonly string prefix = ">";
    static readonly string dot = "<sprite index=1 tint>";
    void Start() {
        foreach (Transform child in logTextHolder) {
            Destroy(child.gameObject);
        }
        promptText.text = prefix;
    }

    override public void Bind(GameObject newTargetObject) {
        if (target != null) {
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
    override public void OnDestroy() {
        if (target != null) {
            target.OnValueChanged -= HandleValueChanged;
            target.OnActionDone -= HandleActionDone;
        }
    }
    override public void HandleValueChanged(Interactor interactor) {
        InteractorTargetData newData = interactor.ActiveTarget();
        if (!InteractorTargetData.Equality(data, newData)) {
            Disable();
            data = newData;
            DataChanged();
        }
    }
    void HandleActionDone(InteractorTargetData data) {
        CreateLogEntry($"{prefix}{data.target.actionPrompt}\n{data.target.ResponseString()}");
    }
    public void ShowMessage(string entry) {
        CreateLogEntry($"{prefix}{entry}");
    }
    void CreateLogEntry(string entry) {
        GameObject logEntry = GameObject.Instantiate(logTextEntryPrefab);
        TextMeshProUGUI logText = logEntry.GetComponent<TextMeshProUGUI>();
        logText.text = entry;
        logEntry.transform.SetParent(logTextHolder, false);

        // TODO: configurable log cull time
        Destroy(logEntry, 10);
    }
    void Disable() {
        promptText.text = prefix;
        if (blitTextCoroutine != null) {
            StopCoroutine(blitTextCoroutine);
            audioSource.Stop();
        }
    }
    void DataChanged() {
        Disable();
        if (data != null) {
            blitTextCoroutine = StartCoroutine(BlitCalloutText(data.target.actionPrompt));
        }
    }
    public IEnumerator BlitCalloutText(string actionText) {
        float blitInterval = 0.01f;
        float timer = 0f;
        int index = 1;
        string targetText = $"{prefix}{actionText}{dot}";
        audioSource.Play();
        while (promptText.text != targetText) {
            while (timer < blitInterval) {
                timer += Time.deltaTime;
                yield return null;
            }
            timer -= blitInterval;
            index += 1;
            if (index > prefix.Length + actionText.Length) {
                index = targetText.Length;
            }
            promptText.text = targetText.Substring(0, index);
        }
        audioSource.Stop();
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
