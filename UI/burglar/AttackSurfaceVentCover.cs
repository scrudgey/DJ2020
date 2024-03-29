using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Obi;
using UnityEngine;
public class AttackSurfaceVentCover : AttackSurfaceElement {
    public AudioSource audioSource;
    public AudioClip[] openSounds;
    public AudioClip[] lockedSounds;
    public List<AttackSurfaceScrew> screws;
    public GameObject[] parentVentObjects;
    public SpriteRenderer ventSprite;
    public bool finishing;
    public GameObject[] obscuredElements;
    public MeshRenderer[] obscuredRenderers;
    public bool ventCoverIsShut;
    void Start() {
        ventCoverIsShut = true;
        foreach (GameObject element in obscuredElements) {
            element.SetActive(false);
        }
        foreach (MeshRenderer renderer in obscuredRenderers) {
            renderer.enabled = false;
        }
    }
    public override BurglarAttackResult HandleSingleClick(BurglarToolType activeTool, BurgleTargetData data) {
        base.HandleSingleClick(activeTool, data);

        if (activeTool == BurglarToolType.none) {
            if (IsLocked()) {
                Toolbox.RandomizeOneShot(audioSource, lockedSounds);
            } else {
                RemovePanel();
                return BurglarAttackResult.None with {
                    success = true,
                    feedbackText = "Vent cover open",
                    finish = finishing,
                    panel = this
                };
            }
        }
        return BurglarAttackResult.None;
    }

    public void RemovePanel() {
        ventCoverIsShut = false;
        Toolbox.AudioSpeaker(transform.position, openSounds);
        ventSprite.enabled = false;
        foreach (GameObject parentVentObject in parentVentObjects) {
            parentVentObject.SetActive(false);
        }
        foreach (GameObject element in obscuredElements) {
            element.SetActive(true);
        }
        foreach (MeshRenderer renderer in obscuredRenderers) {
            renderer.enabled = true;
        }
    }
    public void ReplacePanel() {
        ventCoverIsShut = true;
        Toolbox.AudioSpeaker(transform.position, openSounds);
        ventSprite.enabled = true;
        foreach (GameObject parentVentObject in parentVentObjects) {
            parentVentObject.SetActive(true);
        }
        foreach (GameObject element in obscuredElements) {
            element.SetActive(false);
        }
        foreach (MeshRenderer renderer in obscuredRenderers) {
            renderer.enabled = false;
        }
    }
    public override BurglarAttackResult HandleClickHeld(BurglarToolType activeTool, BurgleTargetData data) {
        base.HandleClickHeld(activeTool, data);
        return BurglarAttackResult.None;
    }

    bool IsLocked() => screws.Any(screw => !screw.unscrewed);
}
