using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerSource : PoweredComponent {
    public AudioSource buzzSoundSource;
    protected override void OnPowerChange() {
        base.OnPowerChange();
        if (buzzSoundSource != null) {
            if (!power) {
                buzzSoundSource?.Stop();
            } else {
                buzzSoundSource?.Play();
            }
        }
    }
    public override void EnableSource() {
        base.EnableSource();
        GameManager.I.SetPowerNodeState(this, true);
    }
    public override void ConfigureNode(PowerNode node) {
        node.powered = true;
    }
    public override void OnDestroy() {
        base.OnDestroy();
        GameManager.I?.SetPowerNodeState(this, false);
    }
}
