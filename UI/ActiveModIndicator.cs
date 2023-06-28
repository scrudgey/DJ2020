using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ActiveModIndicator : MonoBehaviour {
    public Image icon;
    public TextMeshProUGUI description;

    public void Initialize(GunMod mod, Sprite sprite) {
        icon.sprite = sprite;
        description.text = $" {GunModShopController.ModSummary(mod)}";
    }
}
