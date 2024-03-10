using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
[System.Serializable]
public abstract class Tactic : ScriptableObject {
    public Sprite vendorSprite;
    public string vendorName;
    public string title;
    public int cost;
    [TextArea(15, 20)]
    public string decsription;

    [TextArea(15, 20)]
    public string vendorPitch;

    public Sprite icon;

    public virtual void ApplyPurchaseState(LevelTemplate template, LevelPlan plan) {

    }
}