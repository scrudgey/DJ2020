using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/ExplosionData")]
public class ExplosionData : ScriptableObject {
    public GameObject explosionFx;
    public float explosionRadius = 2f;
    public float explosionPower = 10f;
}