using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class AttackSurfaceVentCover : AttackSurfaceElement {
    public AudioSource audioSource;
    public AudioClip[] openSounds;
    public AudioClip[] lockedSounds;
    public List<AttackSurfaceScrew> screws;
    public GameObject parentVentObject;
    public SpriteRenderer ventSprite;
    public override BurglarAttackResult HandleSingleClick(BurglarToolType activeTool, BurgleTargetData data) {
        base.HandleSingleClick(activeTool, data);

        if (activeTool == BurglarToolType.none) {
            if (IsLocked()) {
                Toolbox.RandomizeOneShot(audioSource, lockedSounds);
            } else {
                Toolbox.AudioSpeaker(parentVentObject.transform.position, openSounds);
                ventSprite.enabled = false;
                parentVentObject.SetActive(false);
                return new BurglarAttackResult() {
                    success = true,
                    feedbackText = "Vent cover open",
                    finish = true
                };
            }
        }
        return BurglarAttackResult.None;
    }

    public override BurglarAttackResult HandleClickHeld(BurglarToolType activeTool, BurgleTargetData data) {
        base.HandleClickHeld(activeTool, data);
        return BurglarAttackResult.None;
    }

    bool IsLocked() => screws.Any(screw => !screw.unscrewed);
}
