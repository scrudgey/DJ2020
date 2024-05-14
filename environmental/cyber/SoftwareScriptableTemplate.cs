using System.Collections.Generic;
using System.Linq;
using UnityEngine;
[CreateAssetMenu(menuName = "ScriptableObjects/SoftwareScriptableTemplate")]
public class SoftwareScriptableTemplate : ScriptableObject {
    public new string name;
    public SoftwareEffect.Type principalType;
    public List<SoftwareEffect> effects;
    public bool infinteCharges;
    public int maxCharges;
    public Sprite icon;
    public List<SoftwareCondition> conditions;
    public AudioClip[] deploySounds;
    public SoftwareTemplate ToTemplate() {
        return new SoftwareTemplate() {
            name = name,
            effects = effects,
            maxCharges = maxCharges,
            icon = icon,
            principalType = principalType,
            conditions = conditions,
            deploySounds = deploySounds.ToList(),
            infiniteCharges = infinteCharges
        };
    }

    public static SoftwareScriptableTemplate Load(string name) {
        return Resources.Load($"data/software/{name}") as SoftwareScriptableTemplate;
    }
}