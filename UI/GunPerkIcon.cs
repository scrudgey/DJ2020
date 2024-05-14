using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GunPerkIcon : MonoBehaviour {
    public Image icon;
    public void Initialize(GunPerk perk, GraphIconReference iconReference) {
        icon.sprite = iconReference.GunPerkSprite(perk.type);
    }
}
