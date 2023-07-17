using System.Collections;
using System.Collections.Generic;
using Items;
using UnityEngine;
public enum ExplosiveType { timer, impact }
public class Explosive : MonoBehaviour {
    public ExplosiveType type;
    public AudioClip[] explodeSounds;
    public float lifetime;
    public float timer;
    // public C4Data data;
    public ExplosionData data;
    public static GameObject explosiveRadius;

    void Update() {
        timer += Time.deltaTime;
        if (type == ExplosiveType.timer) {
            if (timer > lifetime) {
                Explode();
            }
        }
    }
    public void Explode() {
        AudioSource source = Toolbox.AudioSpeaker(transform.position, explodeSounds, volume: 5f);
        source.minDistance = 5f;
        source.maxDistance = 10f;

        GameObject.Instantiate(data.explosionFx, transform.position, Quaternion.identity);
        Explosion explosion = Toolbox.Explosion(transform.position);
        explosion.radius = data.explosionRadius;
        explosion.power = data.explosionPower;
        if (transform != null && transform.parent != null && transform.parent.gameObject != null) {
            Destroy(transform.parent.gameObject);
        } else {
            Destroy(gameObject);
        }

    }
}
