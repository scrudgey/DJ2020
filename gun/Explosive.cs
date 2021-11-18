using System.Collections;
using System.Collections.Generic;
using Items;
using UnityEngine;
public enum ExplosiveType { timer }
public class Explosive : MonoBehaviour {
    public ExplosiveType type;
    public AudioClip[] explodeSounds;
    public float lifetime;
    public float timer;
    public C4Data data;
    public static GameObject explosiveRadius;

    void Update() {
        timer += Time.deltaTime;
        if (type == ExplosiveType.timer) {
            if (timer > lifetime) {
                Explode();
            }
        }
    }
    void Explode() {
        AudioSource source = Toolbox.AudioSpeaker(transform.position, explodeSounds, volume: 5f);
        source.minDistance = 5f;
        source.maxDistance = 10f;

        GameObject.Instantiate(data.explosionFx, transform.position, Quaternion.identity);
        Explosion explosion = Toolbox.Explosion(transform.position);
        explosion.radius = data.explosionRadius;
        explosion.power = data.explosionPower;
        Destroy(transform.parent.gameObject);
    }
}
