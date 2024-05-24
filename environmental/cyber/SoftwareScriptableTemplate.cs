using System.Collections.Generic;
using System.Linq;
using UnityEngine;
[CreateAssetMenu(menuName = "ScriptableObjects/SoftwareScriptableTemplate")]
public class SoftwareScriptableTemplate : ScriptableObject {
    public new string name;
    public SoftwareTemplate.SoftwareType type;
    public SoftwareEffect.Type principalType;
    public List<SoftwareEffect> effects;
    public bool infinteCharges;
    public int maxCharges;
    public Sprite icon;
    public List<SoftwareCondition> conditions;
    public AudioClip[] deploySounds;


    [Header("virus")]
    public int hops;
    public int duplication;
    public float loiterTimeLow;
    public float transitTimeLow;

    public SoftwareTemplate ToTemplate() {
        return new SoftwareTemplate() {
            softwareType = type,
            name = name,
            effects = effects,
            maxCharges = maxCharges,
            icon = icon,
            principalType = principalType,
            conditions = conditions,
            deploySounds = deploySounds.ToList(),
            infiniteCharges = infinteCharges,
            virusDup = duplication,
            virusHops = hops,
            loiterTimeLow = loiterTimeLow,
            transitTimeLow = transitTimeLow
        };
    }

    public static SoftwareScriptableTemplate Load(string name) {
        return Resources.Load($"data/software/{name}") as SoftwareScriptableTemplate;
    }
}