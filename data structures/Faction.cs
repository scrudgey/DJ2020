using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Faction")]
public class Faction : ScriptableObject {
    public Sprite logo;
    public string factionName;
    [TextArea(15, 20)]
    public string description;
}