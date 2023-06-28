using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class GunModButton : MonoBehaviour {
    public Image icon;
    public Image outlineImage;
    [HideInInspector]
    public GunMod gunMod;
    private GunModShopController gunModShopController;
    public Color activeModColor;


    public void Initialize(GunModShopController gunModShopController, GunMod gunMod, Sprite sprite) {
        this.gunModShopController = gunModShopController;
        this.gunMod = gunMod;
        icon.sprite = sprite;
    }
    public void OnClick() {
        gunModShopController.OnModButtonClicked(this);
    }

    public void SetEnabledColors() {
        icon.color = activeModColor;
        outlineImage.color = activeModColor;
    }
}
