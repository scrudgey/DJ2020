using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExtractionZone : MonoBehaviour {
    public MeshRenderer meshRenderer;
    public bool isActive;
    void Awake() {
        if (isActive) {
            EnableExtractionZone();
        } else {
            DisableExtractionZone();
        }
    }
    public void EnableExtractionZone() {
        isActive = true;
        meshRenderer.enabled = true;
    }

    public void DisableExtractionZone() {
        isActive = false;
        meshRenderer.enabled = false;
    }
    void OnTriggerEnter(Collider other) {
        if (!isActive)
            return;
        if (other.transform.IsChildOf(GameManager.I.playerObject.transform)) {
            Debug.Log("successful extraction");
        }
    }
    
}
