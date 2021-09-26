using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Item")]
public class Item : ScriptableObject {
    new public string name;
    public string shortName;
    public Sprite image;

    // public string name;
    // public void UseItem(PlayerInp)

    static public Item LoadItem(string name) {
        return Resources.Load($"data/items/{name}") as Item;
    }
}