using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransparentMesh : MonoBehaviour {
    public float timer = 0f;
    public MeshRenderer mesh;
    public Material transparentMaterial;
    public Material material;
    void Start() {
        mesh = GetComponentInChildren<MeshRenderer>();
        if (mesh == null) {
            Destroy(this);
        } else {
            material = mesh.material;
            string inName = material.name.Replace("(Instance)", "").Trim();
            string targetName = $"materials/{inName}_transparent";
            transparentMaterial = Resources.Load(targetName) as Material;
            if (transparentMaterial == null) {
                Destroy(this);
            }
        }
    }
    void Update() {
        if (timer > 0f) {
            timer -= Time.deltaTime;
            if (mesh.material != transparentMaterial)
                mesh.material = transparentMaterial;
        } else {
            if (mesh.material != material)
                mesh.material = material;
        }
    }
}
