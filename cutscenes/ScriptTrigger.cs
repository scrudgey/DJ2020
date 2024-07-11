using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScriptTrigger : MonoBehaviour {
    public string idn;
    void OnCollisionEnter(Collision collision) {
        if (GameManager.I.isLoadingLevel) return;
        DoTrigger();
    }
    void OnKinematicCharacterImpact() {
        if (GameManager.I.isLoadingLevel) return;
        DoTrigger();
    }
    void OnTriggerEnter(Collider other) {
        if (GameManager.I.isLoadingLevel) return;
        if (!Toolbox.GetTagData(other.gameObject).isActor)
            return;
        DoTrigger();
    }

    void DoTrigger() {
        CutsceneManager.I.HandleTrigger(idn);
    }
}
