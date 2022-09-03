using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Listener : MonoBehaviour {
    public GameObject listener;
    public IListener noiseHandler;
    SphereCollider mySphereCollider;
    void Start() {
        noiseHandler = listener.GetComponent<IListener>();
        noiseHandler.listener = this;
        mySphereCollider = gameObject.GetComponent<SphereCollider>();
    }
    private void OnTriggerEnter(Collider other) {
        NoiseComponent noiseComponent = other.GetComponent<NoiseComponent>();
        if (noiseComponent != null) {
            noiseHandler.HearNoise(noiseComponent);
        }
    }
    // public void SetListenRadius(float radius = 0.4f) {
    //     mySphereCollider.radius = radius;
    // }
}
