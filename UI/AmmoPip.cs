using System.Collections;
using System.Collections.Generic;
using Easings;
using UnityEngine;
using UnityEngine.UI;
public class AmmoPip : MonoBehaviour, IPoolable {
    public RectTransform myRect;
    public RectTransform imageRect;
    public RectTransform layoutRect;
    public Image image;
    public Sprite spritePistol;
    public Sprite spriteRifle;
    public Sprite spriteShotgun;
    float initialWidth;
    float initialImageWidth;
    Coroutine coroutine;
    void Awake() {
        // initialWidth = myRect.rect.width;
        // initialImageWidth = imageRect.rect.width;
        initialWidth = 12;
        initialImageWidth = 32;
    }
    void ResetCoroutine(IEnumerator newCoroutine) {
        if (coroutine != null) {
            StopCoroutine(coroutine);
        }
        coroutine = StartCoroutine(newCoroutine);
    }
    public void SetSprite(GunType type) =>
        image.sprite = type switch {
            GunType.smg => spritePistol,
            GunType.pistol => spritePistol,
            GunType.rifle => spriteRifle,
            GunType.shotgun => spriteShotgun,
            _ => spritePistol
        };

    public void Disappear(PrefabPool pool) {
        ResetCoroutine(SqueezeOut(pool));
    }
    IEnumerator SqueezeOut(PrefabPool pool) {
        float appearanceInterval = 0.25f;
        float timer = 0f;
        float framerateTimer = 0f;
        while (timer < appearanceInterval) {
            timer += Time.unscaledDeltaTime;
            framerateTimer += Time.unscaledDeltaTime;


            float imageWidth = (float)PennerDoubleAnimation.BackEaseOut(timer, initialImageWidth, -1f * initialImageWidth, appearanceInterval);
            float rectWidth = (float)PennerDoubleAnimation.BackEaseOut(timer, initialWidth, -1f * initialWidth, appearanceInterval);

            imageRect.localScale = Vector3.one;
            myRect.localScale = Vector3.one;
            imageRect.sizeDelta = new Vector2(imageWidth, 64);
            myRect.sizeDelta = new Vector2(rectWidth, 32);
            if (framerateTimer > 0.1f) {
                framerateTimer -= 0.1f;
                LayoutRebuilder.ForceRebuildLayoutImmediate(layoutRect);
            }
            yield return null;
        }
        pool.RecallObject(gameObject);
    }


    public void OnPoolActivate() {
        imageRect.localScale = Vector3.one;
        myRect.localScale = Vector3.one;
        imageRect.sizeDelta = new Vector2(initialImageWidth, 64);
        myRect.sizeDelta = new Vector2(initialWidth, 32);
    }
    public void OnPoolDectivate() {

    }
}
