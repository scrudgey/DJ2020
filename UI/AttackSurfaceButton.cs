using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackSurfaceButton : AttackSurfaceElement, INodeBinder<AlarmNode> {
    public AlarmNode node { get; set; }
    [Header("sprites")]
    public Sprite defaultSprite;
    public Sprite mouseOverSprite;
    public Sprite pressedSprite;
    public SpriteRenderer spriteRenderer;
    [Header("audio")]
    public AudioClip[] buttonPressedSound;
    public AudioClip[] buttonMouseOverSound;
    public AudioSource audioSource;

    [Header("effects")]
    public ElevatorController elevatorController;
    public int selectFloor;

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
    public void HandleNodeChange() { }
    public override BurglarAttackResult HandleSingleClick(BurglarToolType activeTool, BurgleTargetData data) {
        base.HandleSingleClick(activeTool, data);
        if (activeTool == BurglarToolType.none) {
            pressTimer = 0.5f;
            Toolbox.RandomizeOneShot(audioSource, buttonPressedSound);
            string feedbackText = "";
            if (node != null) {
                GameManager.I.SetAlarmNodeTriggered(node, false);
                feedbackText = "alarm reset";
            }
            if (elevatorController != null) {
                elevatorController.SelectFloorMove(selectFloor);
            }

            return BurglarAttackResult.None with {
                success = true,
                feedbackText = feedbackText,
                element = this,
            };
        }
        return BurglarAttackResult.None;
    }
}
