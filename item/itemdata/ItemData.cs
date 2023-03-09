using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "ScriptableObjects/Item")]
public class ItemData : ScriptableObject {
    new public string name;
    public string shortName;
    public Sprite image;
    [TextArea(15, 20)]
    public string shopDescription;

    static public ItemData LoadItem(string name) {
        return Resources.Load($"data/items/{name}") as ItemData;
    }

}

