using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
[System.Serializable]
public abstract class Tactic : ScriptableObject {
    [Header("vendor")]
    public Sprite vendorSprite;
    public string vendorName;
    [TextArea(3, 20)]
    public string vendorDescription;
    [TextArea(3, 20)]
    public string vendorIntroduction;
    [Header("tactic")]
    public string title;
    public int cost;
    [TextArea(3, 20)]
    public string decsription;

    [TextArea(3, 20)]
    public string vendorPitch;

    public Sprite icon;

    public virtual void ApplyPurchaseState(LevelTemplate template, LevelPlan plan) {

    }
}