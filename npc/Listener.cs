using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Listener : MonoBehaviour {
    public GameObject listener;
    public IListener noiseHandler;
    void Start() {
        noiseHandler = listener.GetComponent<IListener>();
    }
    private void OnTriggerEnter(Collider other) {
        NoiseComponent noiseComponent = other.GetComponent<NoiseComponent>();
        if (noiseComponent != null) {
            noiseHandler.HearNoise(noiseComponent);
        }
    }
}
