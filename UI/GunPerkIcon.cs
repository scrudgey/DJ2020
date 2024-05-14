using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GunPerkIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    public Image icon;
    Action<GunPerkIcon> mouseOverCallback;
    Action mouseExitCallback;
    public GunPerk perk;
    public void Initialize(GunPerk perk, GraphIconReference iconReference, Action<GunPerkIcon> mouseOverCallback, Action mouseExitCallback) {
        this.mouseOverCallback = mouseOverCallback;
        this.mouseExitCallback = mouseExitCallback;
        this.perk = perk;
        icon.sprite = iconReference.GunPerkSprite(perk.type);
    }

    public virtual void OnPointerEnter(PointerEventData eventData) {
        mouseOverCallback?.Invoke(this);
    }
    public virtual void OnPointerExit(PointerEventData eventData) {
        mouseExitCallback?.Invoke();
    }
}
