using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlarmTerminal : AlarmComponent {
    public MeshRenderer meshRenderer;
    public virtual void Activate() {
        Debug.Log("activating alarm terminal");

        // meshRenderer.material.color = Color.red;
        // meshRenderer.enabled = false;
    }
}
