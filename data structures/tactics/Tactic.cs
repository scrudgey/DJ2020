using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
[System.Serializable]
public abstract class Tactic : ScriptableObject {
    public string title;
    public int cost;
    [TextArea(15, 20)]
    public string decsription;
    public Sprite icon;
}