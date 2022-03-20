using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class NoiseComponent : MonoBehaviour {
    public NoiseData data;
    public SphereCollider sphereCollider;
    public void Start() {
        sphereCollider.radius = data.volume;
    }
}