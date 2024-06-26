using System.Collections;
using System.Collections.Generic;
using Items;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class LoadoutGearSlotButton : MonoBehaviour {
    public Image weaponImage;
    public ItemTemplate item;
    public TextMeshProUGUI itemName;
    public int index;
    CharacterView characterView;

    public void Initialize(CharacterView characterView, int index) {
        this.index = index;
        this.characterView = characterView;
    }
    public void SetItem(ItemTemplate item) {
        if (item == null) {
            Clear();
            return;
        }
        this.item = item;
        weaponImage.enabled = true;
        weaponImage.sprite = item.image;
        itemName.text = item.shortName;
    }
    public void Clear() {
        weaponImage.enabled = false;
        itemName.text = "";
    }
    public void OnClick() {
        characterView.ItemButtonClicked(this);
    }
}
