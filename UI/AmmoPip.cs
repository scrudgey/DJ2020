using System.Collections;
using System.Collections.Generic;
using Easings;
using UnityEngine;
using UnityEngine.UI;
public class AmmoPip : MonoBehaviour {
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
        initialWidth = myRect.rect.width;
        initialImageWidth = imageRect.rect.width;
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

    public void Disappear() {
        ResetCoroutine(SqueezeOut());
    }
    IEnumerator SqueezeOut() {
        float appearanceInterval = 0.25f;
        float timer = 0f;
        while (timer < appearanceInterval) {
            timer += Time.unscaledDeltaTime;

            float imageWidth = (float)PennerDoubleAnimation.BackEaseOut(timer, initialImageWidth, -1f * initialImageWidth, appearanceInterval);
            float rectWidth = (float)PennerDoubleAnimation.BackEaseOut(timer, initialWidth, -1f * initialWidth, appearanceInterval);

            imageRect.sizeDelta = new Vector2(imageWidth, imageRect.rect.height);
            myRect.sizeDelta = new Vector2(rectWidth, myRect.rect.height);
            LayoutRebuilder.ForceRebuildLayoutImmediate(layoutRect);

            yield return null;
        }
        Destroy(gameObject);
    }
}
