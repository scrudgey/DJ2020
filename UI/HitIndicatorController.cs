using System.Collections;
using System.Collections.Generic;
using Easings;
using UnityEngine;
using UnityEngine.UI;
public class HitIndicatorController : MonoBehaviour {
    public List<Image> indicators;
    public float timer;
    public Color baseColor;
    private CharacterHurtable target;
    public void Bind(GameObject target) {
        CharacterHurtable hurtable = target.GetComponentInChildren<CharacterHurtable>();
        if (hurtable == null)
            return;
        if (this.target != null) {
            this.target.OnDamageTaken -= HandleDamageTaken;
        }
        this.target = hurtable;
        hurtable.OnDamageTaken += HandleDamageTaken;
        timer = 2f;
    }
    void OnDestroy() {
        if (target != null) {
            target.OnDamageTaken -= HandleDamageTaken;
        }
    }
    public void HandleDamageTaken(Damage damage) {
        ShowHitIndicator();
    }
    public void ShowHitIndicator() {
        if (timer < 1.5f)
            return;
        timer = 0f;
        SetColors();
    }
    public void Update() {
        if (timer < 1.5f) {
            timer += Time.unscaledDeltaTime;
            if (timer < 1f) {
                SetColors();
            }
        }
    }
    public void SetColors() {
        float alpha = (float)PennerDoubleAnimation.BackEaseOut(timer, 1, -1, 1);
        Color color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
        foreach (Image image in indicators) {
            image.color = color;
        }
    }
}
