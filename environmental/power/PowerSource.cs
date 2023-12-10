using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerSource : PoweredComponent {
    public AudioSource buzzSoundSource;
    public override void HandleNodeChange() {
        base.HandleNodeChange();
        if (buzzSoundSource != null) {
            if (!node.powered) {
                buzzSoundSource?.Stop();
            } else {
                buzzSoundSource?.Play();
            }
        }
    }
    public override PowerNode NewNode() {
        PowerNode node = base.NewNode();
        node.powered = true;
        node.type = NodeType.powerSource;
        return node;
    }

    // TODO: ??
    public override void EnableSource() {
        base.EnableSource();
        GameManager.I.SetPowerNodeState(this, true);
    }
    public override void OnDestroy() {
        base.OnDestroy();
        GameManager.I?.SetPowerNodeState(this, false);
    }
}
