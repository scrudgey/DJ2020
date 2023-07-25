using System.Collections;
using System.Collections.Generic;
using Items;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class LoadoutGearSlotButton : MonoBehaviour {
    public int slotIndex;
    public MissionPlanLoadoutController loadoutController;
    public Image weaponImage;
    public ItemInstance item;
    int index;

    public void Initialize(MissionPlanLoadoutController controller, int index) {
        this.loadoutController = controller;
        this.index = index;
    }
    public void SetItem(ItemInstance item) {
        if (item == null) {
            Clear();
            return;
        }
        this.item = item;
        weaponImage.enabled = true;
        weaponImage.sprite = item.template.image;
    }
    public void Clear() {
        weaponImage.enabled = false;
    }
    public void OnClick() {
        loadoutController.ItemSlotClicked(index, this);
    }
}
