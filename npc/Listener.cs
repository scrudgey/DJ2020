using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Listener : MonoBehaviour {
    public GameObject listener;
    public IListener noiseHandler;
    SphereCollider mySphereCollider;
    RaycastHit[] raycastHits;

    void Start() {
        raycastHits = new RaycastHit[10];
        noiseHandler = listener.GetComponent<IListener>();
        noiseHandler.listener = this;
        mySphereCollider = gameObject.GetComponent<SphereCollider>();
    }
    private void OnTriggerEnter(Collider other) {
        NoiseComponent noiseComponent = other.GetComponent<NoiseComponent>();
        if (noiseComponent != null) {
            if (noiseComponent.data.relevantParties.Contains(transform.root)) return;
            Vector3 position = transform.position;
            Vector3 otherPosition = other.transform.position;
            float distance = Vector3.Distance(otherPosition, position);
            Vector3 direction = otherPosition - position;

            Ray ray = new Ray(position, direction);

            int numberHits = Physics.RaycastNonAlloc(ray, raycastHits, distance * 0.99f, LayerUtil.GetLayerMask(Layer.def, Layer.obj, Layer.interactive), QueryTriggerInteraction.Ignore);
            float volume = noiseComponent.data.volume;
            // Debug.Log($"initial volume: {volume} gun: {noiseComponent.data.isGunshot}");
            foreach (RaycastHit hit in raycastHits) {
                volume = 0.9f * volume;
                float verticality = hit.normal.y / Mathf.Max(1f, new Vector2(hit.normal.x, hit.normal.z).magnitude);
                // Debug.Log($"verticality: {verticality}");
                if (verticality > 1f) {
                    volume *= 0.5f;
                }
            }
            if (volume > 0.5f) {
                noiseHandler.HearNoise(noiseComponent);
            }
            // Debug.Log($"final volume: {volume}");
        }
    }
}
