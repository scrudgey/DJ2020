using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackSurfaceHVACEntry : AttackSurfaceElement {
    public HVACNetwork network;
    public HVACElement startElement;

    public override BurglarAttackResult HandleSingleClick(BurglarToolType activeTool, BurgleTargetData data) {
        base.HandleSingleClick(activeTool, data);
        if (activeTool == BurglarToolType.none) {
            return BurglarAttackResult.None with {
                activateHVACNetwork = true,
                HVACNetwork = network,
                HVACStartElement = startElement
            };
        }
        return BurglarAttackResult.None;
    }
}
