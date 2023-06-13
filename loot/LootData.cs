using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Loot")]
public class LootData : ScriptableObject {
    public Sprite portrait;
    public string lootName;
    public string lootDescription;
    public int value;
    public LootCategory category;
}

