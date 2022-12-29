using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExtractionZone : MonoBehaviour {
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
            GameManager.I.FinishMission(true);
        }
    }
    IEnumerator PulseExtractionZone() {
        float timer = 0f;
        Renderer renderer = GetComponentInChildren<MeshRenderer>();
        Material material = renderer.material;
        // Color color = Color.red;
        Color color = new Color(0.25f, 0f, 0.02f);
        while (true) {
            timer += Time.unscaledDeltaTime;
            // float alpha = Mathf.Abs(Mathf.Sin(timer));
            // float alpha = Mathf.PingPong(timer / 2f, 0.5f);
            float alpha = Mathf.PingPong(timer / 2f, 1f);
            // color.a = alpha;
            // material.SetColor("_Color", color);

            // float emission = Mathf.Abs(Mathf.Sin(timer));
            // Color baseColor = Color.red; //Replace this with whatever you want for your base color at emission level '1'
            // Color finalColor = baseColor * Mathf.LinearToGammaSpace(emission);
            // Debug.Log(finalColor);
            // material.SetColor("_EmissionColor", color);
            material.SetVector("_EmissionColor", color * alpha);
            yield return null;
        }
    }
}
