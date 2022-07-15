using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomizeScale : MonoBehaviour {
    public LoHi scale;
    void Start() {
        DoRandomize();
    }
    void DoRandomize() {
        transform.localScale = scale.Random() * Vector3.one;
        Destroy(this);
    }
}
