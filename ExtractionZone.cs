using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExtractionZone : MapMarker {
    public MeshRenderer meshRenderer;
    public bool isActive;
    public bool showCutscene;
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
        StartCoroutine(PulseExtractionZone());
        if (showCutscene) {
            GameManager.I.ShowExtractionZoneCutscene(this);
        }
    }

    public void DisableExtractionZone() {
        isActive = false;
        meshRenderer.enabled = false;
    }
    void OnTriggerEnter(Collider other) {
        if (!isActive || other.isTrigger)
            return;
        if (other.transform.IsChildOf(GameManager.I.playerObject.transform)) {
            Debug.Log("successful extraction");
            GameManager.I.FinishMission();
        }
    }
    IEnumerator PulseExtractionZone() {
        float timer = 0f;
        Renderer renderer = GetComponentInChildren<MeshRenderer>();
        Material material = renderer.material;
        Color color = new Color(0.25f, 0f, 0.02f);
        while (true) {
            timer += Time.unscaledDeltaTime;
            float alpha = Mathf.PingPong(timer / 2f, 1f);
            material.SetVector("_EmissionColor", color * alpha);
            yield return null;
        }
    }
}
