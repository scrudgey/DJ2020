using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/SoftwareScriptableTemplate")]
public class SoftwareScriptableTemplate : ScriptableObject {
    public new string name;
    public SoftwareEffect.Type principalType;
    public List<SoftwareEffect> effects;
    public int maxCharges;
    public Sprite icon;
    public List<SoftwareCondition> conditions;
    public SoftwareTemplate ToTemplate() {
        return new SoftwareTemplate() {
            name = name,
            effects = effects,
            maxCharges = maxCharges,
            icon = icon,
            principalType = principalType,
            conditions = conditions
        };
    }

    public static SoftwareScriptableTemplate Load(string name) {
        return Resources.Load($"data/software/{name}") as SoftwareScriptableTemplate;
    }
}