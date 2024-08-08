using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScriptTrigger : MonoBehaviour {
    public string idn;
    void OnCollisionEnter(Collision collision) {
        if (GameManager.I.isLoadingLevel) return;
        if (!collision.gameObject.CompareTag("actor")) return;
        if (collision.transform.IsChildOf(GameManager.I.playerObject.transform)) {
            DoTrigger();
        }
    }
    void OnTriggerEnter(Collider other) {
        // Debug.Log($"[script trigger] on trigger enter: {other}");
        if (GameManager.I.isLoadingLevel) return;
        if (!other.gameObject.CompareTag("actor")) return;
        if (other.transform.IsChildOf(GameManager.I.playerObject.transform)) {
            DoTrigger();
        }
    }
    void DoTrigger() {
        CutsceneManager.I.HandleTrigger(idn);
    }
}
