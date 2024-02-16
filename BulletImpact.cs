using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletImpact : MonoBehaviour {
    public Damage damage;
    public Transform impacted;

    public void DestroyInTime(float timer, PrefabPool pool) {
        StartCoroutine(WaitAndRecall(timer, pool));
    }

    IEnumerator WaitAndRecall(float timer, PrefabPool pool) {
        yield return new WaitForSeconds(timer);
        pool.RecallObject(gameObject);
    }
}
