using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UI : Singleton<UI> {
    public GameObject focus;
    private GunHandler focusGunHandler;
    public Canvas canvas;
    [Header("Elements")]
    public TextMeshProUGUI ammoIndicator;
    public void SetFocus(GameObject focus) {
        this.focus = focus;
        this.focusGunHandler = focus.GetComponentInChildren<GunHandler>();
    }
    void Start() {
        canvas.worldCamera = Camera.main;
    }
    void Update() {
        if (focus == null)
            return;

        // gun display
        if (focusGunHandler != null && focusGunHandler.gunInstance != null) {
            ammoIndicator.text = $"Ammo: {focusGunHandler.gunInstance.TotalAmmo()}";
        } else {
            ammoIndicator.text = $"Ammo: -";
        }
    }
}
