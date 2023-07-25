using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "ScriptableObjects/Item")]
public class ItemTemplate : ScriptableObject {
    new public string name;
    public string shortName;
    public Sprite image;
    [TextArea(15, 20)]
    public string shopDescription;

    static public ItemTemplate LoadItem(string name) {
        return Resources.Load($"data/items/{name}") as ItemTemplate;
    }

}

