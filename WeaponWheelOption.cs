using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class WeaponWheelOption : MonoBehaviour {
    public Image icon;
    public Image background;
    public string optionName;
    public Color normalColor;
    public Color highlightColor;

    public GunState gun;

    public void Initialize(GunState gun) {
        this.gun = gun;
        icon.sprite = gun.GetSprite();
        background.color = normalColor;
        optionName = gun.getName();
    }

    public void HandleMouseOver(bool mouseOver) {
        background.color = mouseOver ? highlightColor : normalColor;
    }
}
