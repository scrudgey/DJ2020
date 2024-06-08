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
    string fromId;
    string toId;
    public void Initialize(string fromId, string toId) {
        this.fromId = fromId;
        this.toId = toId;
    }
    public override BurglarAttackResult HandleSingleClick(BurglarToolType activeTool, BurgleTargetData data) {
        base.HandleSingleClick(activeTool, data);
        if (activeTool == BurglarToolType.wirecutter && !isCut) {
            DoCut();
            return BurglarAttackResult.None with {
                feedbackText = $"wire cut {fromId}->{toId}",
                success = true,
                makeTamperEvidenceSuspicious = true
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
    }
}
