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
    public GunState gun;
    public ItemInstance item;
    public bool holster;

    public void Initialize(GunState gun) {
        this.gun = gun;
        icon.sprite = gun.GetSprite();
        background.color = normalColor;
        optionName = gun.getName();
        holsterIcon.SetActive(false);
    }
    public void Initialize(ItemInstance item) {
        this.item = item;
        icon.sprite = item.template.image;
        background.color = normalColor;
        optionName = item.template.name;
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
