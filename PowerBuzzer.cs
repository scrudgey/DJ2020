using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerBuzzer : MonoBehaviour, INodeBinder<PowerNode> {
    public PowerNode node { get; set; }
    public AudioSource buzzSoundSource;
    public void HandleNodeChange() {
        if (buzzSoundSource != null) {
            if (!node.powered) {
                buzzSoundSource?.Stop();
            } else {
                buzzSoundSource?.Play();
            }
        }
    }
}
