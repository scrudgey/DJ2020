using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Faction")]
public class Faction : ScriptableObject {
    public Sprite logo;
    public string factionName;
    public string description;
}