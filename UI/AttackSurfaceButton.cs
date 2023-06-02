using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackSurfaceButton : AttackSurfaceElement {
    public Sprite defaultSprite;
    public Sprite mouseOverSprite;
    public Sprite pressedSprite;
    public SpriteRenderer spriteRenderer;
    public AlarmComponent alarmComponent;
    public AudioClip[] buttonPressedSound;
    public AudioClip[] buttonMouseOverSound;
    public AudioSource audioSource;
    float pressTimer;
    public override void OnMouseOver() {
        if (spriteRenderer.sprite == pressedSprite) return;
        spriteRenderer.sprite = mouseOverSprite;
        Toolbox.RandomizeOneShot(audioSource, buttonMouseOverSound);
    }
    public override void OnMouseExit() {
        if (spriteRenderer.sprite == pressedSprite) return;
        spriteRenderer.sprite = defaultSprite;
    }
    void Update() {
        if (pressTimer > 0) {
            pressTimer -= Time.unscaledDeltaTime;
            if (pressTimer <= 0) {
                spriteRenderer.sprite = defaultSprite;
            } else {
                spriteRenderer.sprite = pressedSprite;
            }
        }
    }
    public override BurglarAttackResult HandleSingleClick(BurglarToolType activeTool, BurgleTargetData data) {
        base.HandleSingleClick(activeTool, data);
        if (activeTool == BurglarToolType.none) {
            GameManager.I.SetAlarmNodeState(alarmComponent, false);
            pressTimer = 0.5f;
            Toolbox.RandomizeOneShot(audioSource, buttonPressedSound);
            return BurglarAttackResult.None with {
                success = true,
                feedbackText = "alarm reset",
                element = this,
            };
        }
        return BurglarAttackResult.None;
    }
}
