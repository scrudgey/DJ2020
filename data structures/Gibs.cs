using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Gibs")]
public class Gibs : ScriptableObject {
    public List<Gib> gibs;
}

[System.Serializable]
public class Gib {
    public LoHi number;
    public GameObject prefab;
    public LoHi velocity;
    public LoHi angleFromHorizontal;
    public void Emit(Damage damage, Collider bounds) {
        // TODO: if glass, handle this differently?
        int num = (int)Toolbox.RandomFromLoHi(number);
        for (int i = 0; i < num; i++) {
            DoEmit(damage, bounds);
        }
    }
    void DoEmit(Damage damage, Collider bounds) {
        Vector3 position = Toolbox.RandomInsideBounds(bounds);
        // Vector3 force = Toolbox.CalculateExplosionVector(explosion, position);
        Vector3 force = damage.GetDamageAtPoint(position);
        DoEmit(position, force);
    }
    void DoEmit(Vector3 position, Vector3 direction) {
        GameObject bit = PoolManager.I.GetPool(prefab).GetObject(position);
        Rigidbody rigidbody = bit.GetComponent<Rigidbody>();
        Vector3 force = Toolbox.RandomFromLoHi(velocity) * direction;
        rigidbody.AddForce(force, ForceMode.Impulse);
    }

}