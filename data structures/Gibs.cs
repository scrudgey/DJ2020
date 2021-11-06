using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Gibs")]
public class Gibs : ScriptableObject {
    public List<Gib> gibs;
    public void Emit(Damage damage, Collider bounds) {
        foreach (Gib gib in gibs) {
            // Debug.Log("emit gib")
            gib.Emit(damage, bounds);
        }
        // Debug.Break();
    }
}

[System.Serializable]
public class Gib {
    public LoHi number;
    public GameObject prefab;
    public LoHi velocity;
    public LoHi dispersion;
    public float directional = 1f;
    public void Emit(Damage damage, Collider bounds) {
        int num = (int)Toolbox.RandomFromLoHi(number);
        for (int i = 0; i < num; i++) {
            DoEmit(damage, bounds);
        }
    }
    void DoEmit(Damage damage, Collider bounds) {
        Vector3 position = Toolbox.RandomInsideBounds(bounds);
        Vector3 force = damage.GetDamageAtPoint(position);
        DoEmit(position, force);
    }
    void DoEmit(Vector3 position, Vector3 inDirection) {
        GameObject bit = PoolManager.I.GetPool(prefab).GetObject(position);
        Rigidbody rigidbody = bit.GetComponent<Rigidbody>();

        Vector3 direction = (directional * inDirection) + ((1 - directional) * Vector3.up);
        direction = (Toolbox.RandomFromLoHi(dispersion) * Toolbox.RandomPointOnPlane(Vector3.zero, direction, 1f)) + direction.normalized;
        direction = direction.normalized * Toolbox.RandomFromLoHi(velocity);

        rigidbody.AddForce(direction, ForceMode.Force);
    }

}