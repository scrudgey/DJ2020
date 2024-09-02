using System.Collections;
using System.Collections.Generic;
using Easings;
using UnityEngine;

public class CuttableFence : MonoBehaviour {
    public int health;
    public PlaySound playSound;
    public Transform intactFenceMeshTransform;
    public GameObject intactFence;
    public AudioClip[] brokenFenceSound;
    public Gibs brokenFenceGibs;
    public GameObject sabotagedFence;
    public Collider bounds;
    public Transform gibEmitPoint;
    [Header("shake params")]
    public float frequency;
    public float halfLife;
    bool broken;
    Coroutine shakeRoutine;
    // Vector3 base
    public void TakeDamage(int amount, Vector3 direction) {
        health -= amount;
        playSound.DoPlay();
        if (shakeRoutine != null) {
            StopCoroutine(shakeRoutine);
        }
        shakeRoutine = StartCoroutine(shakeFence());
        if (health <= 0 && !broken) {
            BreakFence(direction);
        }
    }

    IEnumerator shakeFence() {
        Vector3 direction = gibEmitPoint.forward;
        float timer = 0f;
        float duration = 0.3f;
        while (timer < duration) {
            timer += Time.deltaTime;
            float amount = 0.1f * Toolbox.DecayingCos(timer, halfLife, frequency);
            Vector3 displacement = direction * amount;
            intactFenceMeshTransform.localPosition = displacement;
            yield return null;
        }
        intactFenceMeshTransform.localPosition = Vector3.zero;
    }

    void BreakFence(Vector3 direction) {
        broken = true;
        Vector3 damageDirection = Random.insideUnitSphere;
        damageDirection.x = 0f;
        damageDirection.z = 0f;
        damageDirection += direction;
        Damage damage = new Damage(0.1f, damageDirection, gibEmitPoint.position, gibEmitPoint.position - direction);
        brokenFenceGibs.DoEmit(gameObject, damage, bounds);
        intactFence.SetActive(false);
        sabotagedFence.SetActive(true);
        Toolbox.AudioSpeaker(transform.position, brokenFenceSound);

        CutsceneManager.I.HandleTrigger($"fence cut {gameObject.name}");
    }
}
