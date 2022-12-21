using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackSurfaceElement : MonoBehaviour {
    public string elementName;
    public virtual void HandleAttack(BurglarToolType activeTool, BurgleTargetData data) {
        Debug.Log($"{activeTool} -> {elementName}");
    }
}
