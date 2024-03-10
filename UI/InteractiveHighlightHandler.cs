using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InteractiveHighlightHandler : IBinder<Interactor> {
    public Camera cam;
    public RectTransform cursor;
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
    void Awake() {
        blitTextCoroutine = null;
    }
    override public void HandleValueChanged(Interactor interactor) {
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
        if (currentAttackSurface != interactor.selectedAttackSurface) {
            currentAttackSurface?.DisableOutline();
        }
        currentAttackSurface = interactor.selectedAttackSurface;
        if (currentAttackSurface != null) {
            currentAttackSurface.EnableOutline();
            interactButton.gameObject.SetActive(true);

            Vector3 screenPoint = cam.WorldToScreenPoint(currentAttackSurface.attackElementRoot.position);
            interactButtonRect.position = screenPoint;
            bool interactible = GameManager.I.playerCharacterController.state == CharacterState.normal ||
                                GameManager.I.playerCharacterController.state == CharacterState.wallPress;
            interactible &= Vector3.Distance(currentAttackSurface.attackElementRoot.position, GameManager.I.playerPosition) < currentAttackSurface.interactionDistance;
            interactButton.interactable = interactible;
        } else {
            interactButton.gameObject.SetActive(false);
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
    }
    public void InteractButtonCallback() {
        if (currentAttackSurface) {
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
