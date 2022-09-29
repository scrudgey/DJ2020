using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class AmmoSpent : MonoBehaviour {
    public RectTransform myRect;
    public Image image;

    public Sprite spritePistol;
    public Sprite spriteRifle;
    public Sprite spriteShotgun;
    Vector3 initialPosition;
    Vector3 gravity = new Vector3(0f, -7500f, 0f);
    Vector3 velocity;
    float lifetime;
    float angularVelocity;
    void Start() {
        initialPosition = myRect.rect.position;
        float vx = Random.Range(0f, 350f);
        float vy = Random.Range(900f, 1200f);
        angularVelocity = Random.Range(-1000f, 1000f);
        velocity = new Vector3(vx, vy, 0f);
        lifetime = Random.Range(0.25f, 0.4f);
    }
    void Update() {
        // Rect rect = myRect.rect;
        Vector3 position = myRect.anchoredPosition;
        position += velocity * Time.unscaledDeltaTime;
        velocity += gravity * Time.unscaledDeltaTime;
        lifetime -= Time.unscaledDeltaTime;
        Quaternion rotation = Quaternion.AngleAxis(angularVelocity * lifetime, new Vector3(0f, 0f, 1f));
        myRect.anchoredPosition = position;
        myRect.rotation = rotation;
        if (lifetime <= 0) {
            Destroy(gameObject);
        }
    }
    public void SetSprite(GunType type) =>
    image.sprite = type switch {
        GunType.smg => spritePistol,
        GunType.pistol => spritePistol,
        GunType.rifle => spriteRifle,
        GunType.shotgun => spriteShotgun,
        _ => spritePistol
    };
}
