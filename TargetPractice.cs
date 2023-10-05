using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetPractice : MonoBehaviour, IDamageReceiver {
    public Transform centerPoint;
    [Header("colliders")]
    public Collider centerCollider;
    public Collider middleCollider;
    public Collider outerCollider;
    public int centerPoints;
    public int middlePoints;
    public int outerPoints;
    public DamageResult TakeDamage(Damage damage) {
        if (damage is BulletDamage) {
            BulletDamage bulletDamage = (BulletDamage)damage;
            Vector3 impactPoint = bulletDamage.position;
            int points = 0;
            if (centerCollider.bounds.Contains(impactPoint)) {
                points = centerPoints;
            } else if (middleCollider.bounds.Contains(impactPoint)) {
                points = middlePoints;
            } else if (outerCollider.bounds.Contains(impactPoint)) {
                points = outerPoints;
            }
            float distance = Vector3.Distance(centerPoint.position, impactPoint);
            Debug.Log($"{points} : {distance}");
            TargetHitData data = new TargetHitData {
                points = points,
                distance = distance
            };
            TargetPracticeUIHandler.OnTargetHit?.Invoke(data);

        }
        return DamageResult.NONE;
    }
}
