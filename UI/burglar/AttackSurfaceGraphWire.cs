using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackSurfaceGraphWire : AttackSurfaceElement { //, INodeBinder<PowerNode>, INodeBinder<CyberNode>, INodeBinder<AlarmNode> {
    public SpriteRenderer spriteRenderer;
    public Sprite cutSprite;
    public bool isCyber;
    public bool isAlarm;
    public bool isPower;
    bool isCut;
    public string fromId;
    public string toId;
    public void Initialize(string fromId, string toId) {
        this.fromId = fromId;
        this.toId = toId;
    }
    public override BurglarAttackResult HandleSingleClick(BurglarToolType activeTool, BurgleTargetData data) {
        base.HandleSingleClick(activeTool, data);
        if (activeTool == BurglarToolType.wirecutter && !isCut) {
            DoCut();
            return BurglarAttackResult.None with {
                feedbackText = $"wire cut",
                success = true,
                makeTamperEvidenceSuspicious = true
            };
        } else if (activeTool == BurglarToolType.usb) {
            // Toolbox.RandomizeOneShot(audioSource, attachSound);
            return BurglarAttackResult.None with {
                success = true,
                feedbackText = "connected wire",
                element = this,
                // attachedCyberNode = node
            };
        } else {
            return BurglarAttackResult.None;
        }
    }

    void DoCut() {
        isCut = true;
        spriteRenderer.sprite = cutSprite;
        if (isCyber) {
            GameManager.I.SetCyberEdgeDisabled(fromId, toId);
        }
        if (isPower) {
            GameManager.I.SetPowerEdgeDisabled(fromId, toId);
        }
        if (isAlarm) {
            GameManager.I.SetAlarmEdgeDisabled(fromId, toId);
        }
        CutsceneManager.I.HandleTrigger($"wire_cut");
    }
}
