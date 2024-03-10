using System.Collections;
using System.Collections.Generic;
using Items;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class LoadoutGearSlotButton : MonoBehaviour {
    public MissionPlanLoadoutController loadoutController;
    public Image weaponImage;
    public ItemTemplate item;
    public TextMeshProUGUI itemName;
    int index;

    public void Initialize(MissionPlanLoadoutController controller, int index) {
        this.loadoutController = controller;
        this.index = index;
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
        loadoutController.ItemSlotClicked(index, this);
    }
}
