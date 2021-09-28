using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType { none, deck, c4 }

[CreateAssetMenu(menuName = "ScriptableObjects/Item")]
public class ItemData : ScriptableObject {
    public ItemType type;
    new public string name;
    public string shortName;
    public Sprite image;

    static public ItemData LoadItem(string name) {
        return Resources.Load($"data/items/{name}") as ItemData;
    }

}

