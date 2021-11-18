using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightFlicker : MonoBehaviour {
    new public Light light;
    private float initialIntensity;
    private Color initialColor;
    public float colorFluctuationIntensity = 1f;
    public float intensityFluctuationIntensity = 1f;
    private Vector3 initialColorVector;
    public Flow colorFlow;
    public Flow intensityFlow;

    void Start() {
        initialIntensity = light.intensity;
        initialColor = light.color;
        initialColorVector = Toolbox.ColorToVector(initialColor);
        colorFlow.currentPosition = Random.insideUnitSphere;
        intensityFlow.currentPosition = Random.insideUnitSphere;
    }

    private void LateUpdate() {
        Vector3 currentPosition = colorFlow.Update(Time.deltaTime);
        Vector3 total = initialColorVector + (currentPosition.normalized * colorFluctuationIntensity);
        light.color = Toolbox.VectorToColor(total);

        Vector3 currentIntensity = intensityFlow.Update(Time.deltaTime) * intensityFluctuationIntensity;
        light.intensity = initialIntensity + Mathf.Abs(currentIntensity.z);
    }

}
