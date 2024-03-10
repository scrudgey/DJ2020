using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public record NoiseData {
    public float volume;
    public Suspiciousness suspiciousness;
    public bool player;
    public float pitch;
    public bool isGunshot;
    public bool isFootsteps;
    public Ray ray;
    public GameObject source;
    public HashSet<Transform> relevantParties = new HashSet<Transform>();
}