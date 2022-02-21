using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereRobotAnimator : MonoBehaviour {
    public float floatMeanHeight = 1f;
    public float floatFrequency = 1f;
    public float floatAmplitude = 0.1f;
    public float floatStep = 0.02f;

    private float timer;
    void Update() {
        timer += Time.deltaTime;
        float y = FloatAmount(timer);
        transform.localPosition = new Vector3(0f, y, 0f);
    }

    float FloatAmount(float time) {
        float amount = floatMeanHeight + floatAmplitude * Mathf.Sin(time * floatFrequency);
        float discreteAmount = floatStep * (int)(amount / floatStep);
        return discreteAmount;
    }
}
