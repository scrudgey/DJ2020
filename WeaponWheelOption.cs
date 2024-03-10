using System.Collections;
using System.Collections.Generic;
using Items;
using UnityEngine;
using UnityEngine.UI;

public class WeaponWheelOption : MonoBehaviour {
    public Image icon;
    public Image background;
    public string optionName;
    public Color normalColor;
    public Color highlightColor;
    // public Color holsterColor;
    // public Sprite holsterSprite;
    public GameObject holsterIcon;
    public WeaponState weapon;
    public ItemTemplate item;
    public bool holster;

    public void Initialize(WeaponState weapon) {
        this.weapon = weapon;
        icon.sprite = weapon.GetSprite();
        background.color = normalColor;
        optionName = weapon.GetName();
        holsterIcon.SetActive(false);
    }
    public void Initialize(ItemTemplate item) {
        this.item = item;
        icon.sprite = item.image;
        background.color = normalColor;
        optionName = item.name;
        holsterIcon.SetActive(false);
    }
    public void InitializeHolster() {
        background.color = normalColor;
        optionName = "holster";
        holster = true;
        icon.gameObject.SetActive(false);
        holsterIcon.SetActive(true);
    }

    public void HandleMouseOver(bool mouseOver) {
        background.color = mouseOver ? highlightColor : normalColor;
    }
}
