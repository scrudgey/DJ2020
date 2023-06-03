using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AttackSurfaceLatchGuard : AttackSurfaceElement {
    public Sprite screwedPlateSprite;
    public Sprite noScrewPlateSprite;
    public SpriteRenderer coverSprite;
    public List<AttackSurfaceScrew> screws;

    [Header("sfx")]
    public AudioSource audioSource;
    public AudioClip[] openSounds;

    bool defeated;

    public override void Initialize(AttackSurfaceUIElement uIElement) {
        base.Initialize(uIElement);
        if (defeated) {
            uiElement.gameObject.SetActive(false);
        }
    }
    public void Configure(bool allowScrews) {
        if (allowScrews) {
            coverSprite.sprite = screwedPlateSprite;
            foreach (AttackSurfaceScrew screw in screws) {
                screw.gameObject.SetActive(true);
            }
        } else {
            coverSprite.sprite = noScrewPlateSprite;
            foreach (AttackSurfaceScrew screw in screws) {
                screw.gameObject.SetActive(false);
            }
        }
    }

    public override BurglarAttackResult HandleSingleClick(BurglarToolType activeTool, BurgleTargetData data) {
        base.HandleSingleClick(activeTool, data);
        if (activeTool == BurglarToolType.none) {
            if (IsLocked()) {
                return BurglarAttackResult.None with {
                    success = false,
                    feedbackText = "Latch guard is secured",
                };
            } else {
                Toolbox.AudioSpeaker(transform.position, openSounds);
                defeated = true;
                coverSprite.enabled = false;
                uiElement.gameObject.SetActive(false);
                return BurglarAttackResult.None with {
                    success = true,
                    feedbackText = "Latch guard removed",
                    makeTamperEvidenceSuspicious = true,
                    revealTamperEvidence = true
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
