using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class LootTypeIcon : MonoBehaviour {
    private LootCategory category;
    public Image icon;
    public GraphIconReference iconReference;
    // static Sprite[] icons;
    // void Awake() {
    //     if (icons == null || icons.Length == 0)
    //         icons = Resources.LoadAll<Sprite>("sprites/loot/loot_icons") as Sprite[];
    // }
    public void SetLootCategory(LootCategory category) {
        this.category = category;
        // icon.sprite = LootCategoryToSprite(category);
        icon.sprite = iconReference.LootSprite(category);
    }
    public void Hide() {
        icon.enabled = false;
    }
    public void Show() {
        icon.enabled = true;
    }

    // public static Sprite LootCategoryToSprite(LootCategory category) {
    //     if (icons == null || icons.Length == 0)
    //         icons = Resources.LoadAll<Sprite>("sprites/loot/loot_icons") as Sprite[];
    //     return category switch {
    //         LootCategory.commercial => icons[5],
    //         LootCategory.drug => icons[1],
    //         LootCategory.gem => icons[4],
    //         LootCategory.industrial => icons[2],
    //         LootCategory.medical => icons[3],
    //         _ => icons[0]
    //     };
    // }
}
