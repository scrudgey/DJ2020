using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class AttackSurfaceElement : MonoBehaviour, IBindable<AttackSurfaceElement> {
    public Action<AttackSurfaceElement> OnValueChanged { get; set; }
    public string elementName;
    public float progressPercent;
    public int progressStages = 1;
    public int progressStageIndex = 0;
    public AttackSurfaceUIElement uiElement;
    public virtual void Initialize(AttackSurfaceUIElement uiElement) {
        this.uiElement = uiElement;
    }
    public virtual BurglarAttackResult HandleSingleClick(BurglarToolType activeTool, BurgleTargetData data) {
        return BurglarAttackResult.None;
    }
    public virtual BurglarAttackResult HandleClickHeld(BurglarToolType activeTool, BurgleTargetData data) {
        return BurglarAttackResult.None;
    }

    public virtual void HandleMouseUp() {

    }
    public virtual void HandleFocusLost() {
        OnValueChanged?.Invoke(this);
    }
}
