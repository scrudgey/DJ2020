using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackSurfaceElement : MonoBehaviour {
    public string elementName;
    public virtual BurglarAttackResult HandleSingleClick(BurglarToolType activeTool, BurgleTargetData data) {
        return BurglarAttackResult.None;
    }
    public virtual BurglarAttackResult HandleClickHeld(BurglarToolType activeTool, BurgleTargetData data) {
        return BurglarAttackResult.None;
    }
}
