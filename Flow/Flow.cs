using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Flow {
    public Vector3 currentPosition;
    public float morphSpeed;

    [Range(0f, 1f)]
    public float strength = 1f;

    public bool damping;

    public float frequency = 1f;

    [Range(1, 8)]
    public int octaves = 1;

    [Range(1f, 4f)]
    public float lacunarity = 2f;

    [Range(0f, 1f)]
    public float persistence = 0.5f;

    [Range(1, 3)]
    public int dimensions = 3;

    public NoiseMethodType type;

    private float morphOffset;
    public float resetInterval;
    private float timer;
    public Flow() { }
    // Update is called once per frame
    public Vector3 Update(float deltaTime) {
        timer += deltaTime;
        if (timer > resetInterval) {
            timer = 0;
            currentPosition = Random.insideUnitSphere;
        }

        NoiseMethod method = Noise.methods[(int)type][dimensions - 1];
        float amplitude = damping ? strength / frequency : strength;
        morphOffset += Time.deltaTime * morphSpeed;
        if (morphOffset > 256f) {
            morphOffset -= 256f;
        }

        Vector3 position = currentPosition;

        Vector3 point = new Vector3(position.z, position.y, position.x + morphOffset);
        NoiseSample sampleX = Noise.Sum(method, point, frequency, octaves, lacunarity, persistence);
        sampleX *= amplitude;
        point = new Vector3(position.x, position.z, position.y + morphOffset);
        NoiseSample sampleY = Noise.Sum(method, point, frequency, octaves, lacunarity, persistence);
        sampleY *= amplitude;
        point = new Vector3(position.y, position.x, position.z + morphOffset);
        NoiseSample sampleZ = Noise.Sum(method, point, frequency, octaves, lacunarity, persistence);
        sampleZ *= amplitude;
        Vector3 curl;
        curl.x = sampleZ.derivative.x - sampleY.derivative.y;
        curl.y = sampleX.derivative.x - sampleZ.derivative.y;//+ (1f / (1f + position.y));
        curl.z = sampleY.derivative.x - sampleX.derivative.y;

        currentPosition += Time.deltaTime * curl;
        return currentPosition;
        // Vector3 total = initialColorVector + (currentPosition.normalized * fluctuationIntensity);
        // light.color = Toolbox.VectorToColor(total);

    }
}
