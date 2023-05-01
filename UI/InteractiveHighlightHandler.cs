using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InteractiveHighlightHandler : IBinder<Interactor> {
    // Interactor IBinder<Interactor>.target { get; set; }

    public Camera cam;
    public RectTransform cursor;
    // public HighlightableTargetData currentInteractorTarget;
    public InteractorTargetData currentInteractorTarget;
    public TextMeshProUGUI cursorText;
    public TextMeshProUGUI dotText;
    Coroutine blitTextCoroutine;
    public AudioSource audioSource;
    public Image cursorImage;
    private float timer;
    public Color color;
    public Button interactButton;
    public RectTransform interactButtonRect;
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
                if (GameManager.I.IsObjectVisible(currentInteractorTarget.target.gameObject)) {
                    Enable(currentInteractorTarget.target.calloutText);
                } else {
                    Disable();
                }
            }
        }

    }
    void DataChanged() {
        if (currentInteractorTarget == null) {
            Disable();
        } else if (currentInteractorTarget.target != null) {
            if (GameManager.I.IsObjectVisible(currentInteractorTarget.target.gameObject)) {
                Enable(currentInteractorTarget.target.calloutText);
            } else {
                Disable();
            }
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
            return;
        } else if (currentInteractorTarget != null) {
            Vector3 screenPoint = cam.WorldToScreenPoint(currentInteractorTarget.target.transform.position);
            cursor.position = screenPoint;
            cursorText.color = color;
            dotText.color = color;
            cursorImage.color = color;
            SetScale();
            cursorImage.enabled = false;
        }

        if (currentAttackSurface) {
            Vector3 screenPoint = cam.WorldToScreenPoint(currentAttackSurface.attackElementRoot.position);
            interactButtonRect.position = screenPoint;
            interactButton.interactable = Vector3.Distance(currentAttackSurface.attackElementRoot.position, GameManager.I.playerPosition) < 2f;
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
        if (blitTextCoroutine != null) {
            StopCoroutine(blitTextCoroutine);
        }
        currentInteractorTarget.target.EnableOutline();
        cursorText.enabled = true;

        cursorImage.enabled = true;
        cursorText.text = "";
        dotText.enabled = true;
        blitTextCoroutine = StartCoroutine(BlitCalloutText(actionText));

        AttackSurface[] childAttackSurfaces = currentInteractorTarget.target.transform.root.GetComponentsInChildren<AttackSurface>();
        if (childAttackSurfaces.Length > 0) {
            currentAttackSurface = childAttackSurfaces
                        .OrderBy(attackSurface => Vector3.Distance(attackSurface.transform.position, target.transform.position))
                        .First();
        } else {
            currentAttackSurface = null;
        }

        if (currentAttackSurface) {
            interactButton.gameObject.SetActive(true);
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
