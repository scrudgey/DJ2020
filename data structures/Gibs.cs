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

    public void Emit(Vector3 position, Vector3 direction) {
        GameObject bit = PoolManager.I.GetPool(prefab).GetObject(position);
        Rigidbody rigidbody = bit.GetComponent<Rigidbody>();
        Vector3 force = Toolbox.RandomFromLoHi(velocity) * direction;
        rigidbody.AddForce(force, ForceMode.Impulse);
    }

}