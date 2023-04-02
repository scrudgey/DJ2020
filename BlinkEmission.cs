using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlinkEmission : MonoBehaviour {
    public Renderer myRenderer;
    float timer;
    bool enableEmission;
    void Update() {
        timer += Time.deltaTime;
        if (timer > 2f) {
            enableEmission = !enableEmission;
            timer = 0f;
            // myRenderer.material.GetP
            if (enableEmission) {
                myRenderer.material.EnableKeyword("_EMISSION");
            } else {
                myRenderer.material.DisableKeyword("_EMISSION");
            }
        }
    }
}
