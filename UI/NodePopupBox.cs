using System.Collections;
using System.Collections.Generic;
using Easings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class NodePopupBox<T, U> : MonoBehaviour where T : Node where U : Graph<T, U> {
    enum State { hidden, easeIn, show, easeOut }
    State state;
    float stateTime;
    float targetWidth;
    float targetHeight;
    float startWidth;
    float startHeight;
    public NodeIndicator<T, U> indicator;
    public AudioSource audioSource;
    public RectTransform rectTransform;
    public GameObject[] dataObjects;
    public ContentSizeFitter contentSizeFitter;
    public TextMeshProUGUI idText;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI enabledText;
    public Image boxImage;
    public float transitionTime = 0.5f;
    public float transitionTimeAspectRatio = 1.5f;
    public AudioClip showSound;
    public AudioClip hideSound;
    public Color enabledColor;
    public Color disabledColor;
    void Start() {
        EnterState(state);
        Configure(indicator.node);
        indicator.onMouseOver += HandleNodeMouseOver;
        indicator.onMouseExit += HandleNodeMouseExit;
    }
    void OnDestroy() {
        indicator.onMouseOver -= HandleNodeMouseOver;
        indicator.onMouseExit -= HandleNodeMouseExit;
    }
    public void Configure(T node) {
        SetGraphicalState(node);
    }
    public void HandleNodeMouseOver(NodeIndicator<T, U> indicator) {
        Configure(indicator.node);
        Show();
    }
    public void HandleNodeMouseExit(NodeIndicator<T, U> indicator) {
        Hide();
    }

    protected virtual void SetGraphicalState(T node) {
        idText.text = node.idn.Substring(0, node.idn.Length / 2);
        nameText.text = node.nodeTitle;

        Color activeColor = enabledColor;
        if (node.enabled) {
            enabledText.text = $"Enabled: Y";
            activeColor = enabledColor;
        } else {
            enabledText.text = $"Enabled: N";
            activeColor = disabledColor;
        }

        foreach (GameObject dataObject in dataObjects) {
            TextMeshProUGUI text = dataObject.GetComponent<TextMeshProUGUI>();
            if (text != null)
                text.color = activeColor;
        }
        boxImage.color = activeColor;

    }
    public void Show() {
        switch (state) {
            case State.easeOut:
            case State.hidden:
                ChangeState(State.easeIn);
                break;
            default:
                break;
        }
    }
    public void Hide() {
        switch (state) {
            case State.easeIn:
            case State.show:
                ChangeState(State.easeOut);
                break;
            default:
                break;
        }
    }
    void ChangeState(State newState) {
        if (newState == state) {
            return;
        }
        EnterState(newState);
        state = newState;
    }
    void EnterState(State newState) {
        stateTime = 0;
        startWidth = rectTransform.rect.width;
        startHeight = rectTransform.rect.height;
        switch (newState) {
            case State.show:
                foreach (GameObject dataObject in dataObjects) {
                    dataObject.SetActive(true);
                }
                targetWidth = 100f;
                targetHeight = 74;
                boxImage.enabled = true;
                contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                break;
            case State.easeIn:
                Toolbox.RandomizeOneShot(audioSource, showSound);
                foreach (GameObject dataObject in dataObjects) {
                    dataObject.SetActive(false);
                }
                targetWidth = 100f;
                targetHeight = 74;
                boxImage.enabled = true;
                contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                break;
            case State.hidden:
                foreach (GameObject dataObject in dataObjects) {
                    dataObject.SetActive(false);
                }
                targetWidth = 0f;
                targetHeight = 0f;
                boxImage.enabled = false;
                contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                break;
            case State.easeOut:
                Toolbox.RandomizeOneShot(audioSource, hideSound);
                foreach (GameObject dataObject in dataObjects) {
                    dataObject.SetActive(false);
                }
                targetWidth = 0f;
                targetHeight = 0f;
                boxImage.enabled = true;
                contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                break;
        }
    }
    void Update() {
        stateTime += Time.fixedDeltaTime;
        switch (state) {
            case State.easeIn:
                UpdateEaseIn();
                break;
            case State.easeOut:
                UpdateEaseOut();
                break;
            case State.hidden:
            case State.show:
                break;
        }
    }

    void UpdateEaseIn() {
        if (stateTime > transitionTime * transitionTimeAspectRatio) {
            ChangeState(State.show);
            return;
        }
        UpdateWidthHeight();
    }
    void UpdateEaseOut() {
        if (stateTime > transitionTime * transitionTimeAspectRatio) {
            ChangeState(State.hidden);
            return;
        }
        UpdateWidthHeight();
    }
    void UpdateWidthHeight() {
        float deltaWidth = targetWidth - startWidth;
        float deltaHeight = targetHeight - startHeight;

        float widthTransitionTime = transitionTime * transitionTimeAspectRatio;

        float newWidth = (float)PennerDoubleAnimation.CircEaseIn(Mathf.Min(stateTime, widthTransitionTime), startWidth, deltaWidth, widthTransitionTime);
        float newHeight = (float)PennerDoubleAnimation.CircEaseIn(Mathf.Min(stateTime, transitionTime), startHeight, deltaHeight, transitionTime);

        rectTransform.sizeDelta = new Vector2(newWidth, newHeight);
    }
}
