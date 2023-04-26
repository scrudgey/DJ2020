using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InteractiveHighlightHandler : IBinder<Interactor> {
    // Interactor IBinder<Interactor>.target { get; set; }

    public Camera cam;
    public RectTransform cursor;
    // public HighlightableTargetData currentInteractorTarget;
    public HighlightableTargetData currentInteractorTarget;
    public TextMeshProUGUI cursorText;
    public TextMeshProUGUI dotText;
    Coroutine blitTextCoroutine;
    public AudioSource audioSource;
    public Image cursorImage;
    private float timer;
    public Color color;
    public Button interactButton;
    public AttackSurface currentAttackSurface;
    // public GameObject interactB
    void Awake() {
        blitTextCoroutine = null;
    }
    override public void HandleValueChanged(Interactor interactor) {
        // \
        //  activeTarget = interactor.ActiveTarget();
        // activeTarget = interactor.cursorTarget;
        // if (activeTarget == null) {
        //     Disable();
        //     currentInteractorTarget = null;
        // } else if (activeTarget != null) {
        //     if (!InteractorTargetData.Equality(currentInteractorTarget, activeTarget)) {
        //         currentInteractorTarget?.target?.DisableOutline();
        //         currentInteractorTarget = activeTarget;
        //         DataChanged();
        //     }
        // }

        if (!InteractorTargetData.Equality(currentInteractorTarget, interactor.cursorTarget)) {
            currentInteractorTarget?.target?.DisableOutline();
            currentInteractorTarget = interactor.cursorTarget;
            currentInteractorTarget?.target?.EnableOutline();

            if (currentInteractorTarget == null) {
                Disable();
            } else {
                Enable(currentInteractorTarget.target.calloutText);
            }
        }

    }
    void DataChanged() {
        if (currentInteractorTarget == null) {
            Disable();
        } else if (currentInteractorTarget.target != null) {
            Enable(currentInteractorTarget.target.calloutText);
        }
    }

    void Update() {
        timer += Time.unscaledDeltaTime;
        if (currentInteractorTarget == null) {
            Disable();
            timer = 0f;
        } else if (currentInteractorTarget?.target == null) {
            currentInteractorTarget = null;
            timer = 0f;
            DataChanged();
            if (currentAttackSurface != null) {
                interactButton.interactable = Vector3.Distance(currentAttackSurface.attackElementRoot.position, GameManager.I.playerPosition) < 2f;
            }
            return;
        } else if (currentInteractorTarget != null) {
            Vector3 screenPoint = cam.WorldToScreenPoint(currentInteractorTarget.collider.bounds.center);
            cursor.position = screenPoint;
            cursorText.color = color;
            dotText.color = color;
            cursorImage.color = color;
            SetScale();
            cursorImage.enabled = false;
        }

    }
    void Disable() {
        dotText.enabled = false;
        cursorText.enabled = false;
        cursorImage.enabled = false;
        cursorText.text = "";
        audioSource.Stop();
        if (blitTextCoroutine != null) {
            StopCoroutine(blitTextCoroutine);
        }
        if (currentInteractorTarget != null) {
            currentInteractorTarget.target.DisableOutline();
        }
        interactButton.gameObject.SetActive(false);
    }
    void Enable(string actionText) {
        // Debug.Log($"enable blit text");
        if (blitTextCoroutine != null) {
            StopCoroutine(blitTextCoroutine);
        }
        currentInteractorTarget.target.EnableOutline();
        cursorText.enabled = true;

        cursorImage.enabled = true;
        cursorText.text = "";
        dotText.enabled = true;
        blitTextCoroutine = StartCoroutine(BlitCalloutText(actionText));

        currentAttackSurface = currentInteractorTarget.target.transform.root.GetComponentInChildren<AttackSurface>();
        if (currentAttackSurface) {
            interactButton.gameObject.SetActive(true);
            interactButton.interactable = currentInteractorTarget.targetIsInRange;
            Debug.Log($"interactible: {interactButton.interactable}");
        } else {
            interactButton.gameObject.SetActive(false);
        }
    }
    public void InteractButtonCallback() {
        if (currentAttackSurface) {
            // new BurgleTargetData(kvp.Value, this)
            target.HandleInteractButtonCallback(currentAttackSurface);
        }
    }
    public IEnumerator BlitCalloutText(string actionText) {
        float blitInterval = 0.02f;
        float timer = 0f;
        int index = 1;
        dotText.enabled = true;
        string targetText = $"{actionText}";
        audioSource.Play();
        while (cursorText.text != targetText) {
            while (timer < blitInterval) {
                timer += Time.unscaledDeltaTime;
                yield return null;
            }
            timer -= blitInterval;
            index += 1;
            cursorText.text = targetText.Substring(0, index);
        }
        audioSource.Stop();
        timer = 0f;
        blitInterval = 0.5f;
        while (true) {
            while (timer < blitInterval) {
                timer += Time.unscaledDeltaTime;
                yield return null;
            }
            timer -= blitInterval;
            dotText.enabled = !dotText.enabled;
            yield return null;
        }
    }
    public void SetScale() {
        float distance = Vector3.Distance(cam.transform.position, currentInteractorTarget.target.transform.position);
        float frustumHeight = 2.0f * distance * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);

        float inaccuracyLength = currentInteractorTarget.collider.bounds.size.magnitude / 2f;
        float pixelsPerLength = cam.scaledPixelHeight / frustumHeight;
        float pixelScale = 2f * inaccuracyLength * pixelsPerLength;

        // float dynamicCoefficient = 1.0f + 0.05f * Mathf.Sin(timer);
        // float dynamicCoefficient = Toolbox.Triangle(0.95f, 1.05f, 5f, 0f, timer);
        float dynamicCoefficient = 1f;

        cursor.sizeDelta = dynamicCoefficient * pixelScale * Vector2.one;
    }
}
