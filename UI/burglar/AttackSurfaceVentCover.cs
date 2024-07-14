using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Easings;
using Obi;
using UnityEngine;
public class AttackSurfaceVentCover : AttackSurfaceElement {
    public enum State { closed, open, moving }
    public State state;
    public AudioSource audioSource;
    public AudioClip[] openSounds;
    public AudioClip[] lockedSounds;
    public List<AttackSurfaceScrew> screws;
    public GameObject[] parentVentObjects;
    public SpriteRenderer ventSprite;
    public bool finishing;
    public GameObject[] obscuredElements;
    public MeshRenderer[] obscuredRenderers;
    Vector3 initialPosition;
    Quaternion initialRotation;
    Vector3 initialScale;
    void Start() {
        state = State.closed;
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        initialScale = transform.localScale;
        foreach (GameObject element in obscuredElements) {
            element.SetActive(false);
        }
        if (obscuredRenderers != null)
            foreach (MeshRenderer renderer in obscuredRenderers) {
                if (renderer == null) continue;
                renderer.enabled = false;
            }
    }

    public override BurglarAttackResult HandleSingleClick(BurglarToolType activeTool, BurgleTargetData data) {
        base.HandleSingleClick(activeTool, data);
        if (activeTool == BurglarToolType.none) {
            if (IsLocked()) {
                Toolbox.RandomizeOneShot(audioSource, lockedSounds);
            } else {
                if (state == State.closed) {
                    RemovePanel(data.camera);
                    return BurglarAttackResult.None with {
                        success = true,
                        feedbackText = "Vent cover open",
                        finish = finishing,
                        revealTamperEvidence = true
                    };
                } else if (state == State.open) {
                    ReplacePanel();
                    return BurglarAttackResult.None with {
                        success = true,
                        feedbackText = "Vent cover closed",
                        revealTamperEvidence = false
                    };
                }
            }
        }
        return BurglarAttackResult.None;
    }

    public void RemovePanel(Camera camera) {
        state = State.moving;
        Toolbox.AudioSpeaker(transform.position, openSounds);

        foreach (GameObject parentVentObject in parentVentObjects) {
            if (parentVentObject == null) continue;
            parentVentObject.SetActive(false);
        }
        foreach (GameObject element in obscuredElements) {
            if (element == null) continue;
            element.SetActive(true);
        }
        foreach (MeshRenderer renderer in obscuredRenderers) {
            if (renderer == null) continue;
            renderer.enabled = true;
        }

        StartCoroutine(RemovePanelRoutine(camera));
    }
    public void ReplacePanel() {
        state = State.moving;
        Toolbox.AudioSpeaker(transform.position, openSounds);
        ventSprite.enabled = true;
        foreach (GameObject parentVentObject in parentVentObjects) {
            parentVentObject.SetActive(true);
        }
        foreach (GameObject element in obscuredElements) {
            element.SetActive(false);
        }
        if (obscuredRenderers != null)
            foreach (MeshRenderer renderer in obscuredRenderers) {
                if (renderer == null) continue;
                renderer.enabled = false;
            }
        StartCoroutine(ClosePanelRoutine());
    }


    IEnumerator RemovePanelRoutine(Camera camera) {
        Vector3 displacement = initialPosition - camera.transform.position;
        Vector3 forwardDisplacement = Vector3.Project(displacement, camera.transform.forward);
        Vector3 towardCameraPosition = initialPosition - 0.2f * forwardDisplacement.normalized;

        Quaternion targetRotation = Quaternion.AngleAxis(90f, camera.transform.forward) * initialRotation;

        yield return Toolbox.Ease(null, 0.7f, 0f, 1f, PennerDoubleAnimation.Linear, (amount) => {
            transform.position = Vector3.Lerp(initialPosition, towardCameraPosition, amount);
        });

        yield return Toolbox.Ease(null, 0.45f, 0f, 1f, PennerDoubleAnimation.ExpoEaseOut, (amount) => {
            transform.rotation = Quaternion.Lerp(initialRotation, targetRotation, amount);
        });

        // forwardDisplacement 
        /*
            |-----------------|
            |                 |
            |        *        |
            |*                |
            |-----------------|
         */
        // we need to know the size of the FOV in world units at distance d
        //
        //   /  |          /    |
        //  /   |      /θ/2     | h/2
        // * θ  |     *---------|
        //  \   |           d
        //   \  |
        // tan(θ/2) = h/2d
        // h/2 = d tan(θ/2)

        Vector3 displacement2 = towardCameraPosition - camera.transform.position;
        Vector3 forwardDisplacement2 = Vector3.Project(displacement2, camera.transform.forward);
        float halfHeight = forwardDisplacement2.magnitude * Mathf.Tan(Mathf.Deg2Rad * camera.fieldOfView) / 2;
        float halfWidth = 1.6f * halfHeight;

        Vector3 cornerPosition = (camera.transform.position + forwardDisplacement) - (halfHeight * camera.transform.up) - (halfWidth * camera.transform.right);

        yield return Toolbox.Ease(null, 0.65f, 0f, 1f, PennerDoubleAnimation.ExpoEaseOut, (amount) => {
            transform.position = Vector3.Lerp(towardCameraPosition, cornerPosition, amount);
        });
        state = State.open;
    }

    IEnumerator ClosePanelRoutine() {
        WaitForSecondsRealtime waiter = new WaitForSecondsRealtime(1f);
        transform.position = initialPosition;
        transform.rotation = initialRotation;
        yield return waiter;
        state = State.closed;
    }

    public override BurglarAttackResult HandleClickHeld(BurglarToolType activeTool, BurgleTargetData data) {
        base.HandleClickHeld(activeTool, data);
        return BurglarAttackResult.None;
    }

    bool IsLocked() => screws.Any(screw => !screw.unscrewed);
}
