using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class LootTypeIcon : MonoBehaviour {
    private LootCategory category;
    public Image icon;
    public GraphIconReference iconReference;

    public void SetLootCategory(LootCategory category) {
        this.category = category;
        icon.sprite = iconReference.LootSprite(category);
    }
    public void Hide() {
        icon.enabled = false;
    }
    public void Show() {
        icon.enabled = true;
    }
}
