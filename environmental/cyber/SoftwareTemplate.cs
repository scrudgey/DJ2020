using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

[System.Serializable]
public class SoftwareTemplate {
    public string name;
    public SoftwareEffect.Type principalType;
    public List<SoftwareEffect> effects;
    public bool infiniteCharges;
    public int maxCharges;

    [JsonConverter(typeof(ScriptableObjectJsonConverter<Sprite>))]
    public Sprite icon;

    public List<SoftwareCondition> conditions;

    [JsonConverter(typeof(ObjectListJsonConverter<AudioClip>))]
    public List<AudioClip> deploySounds;

    public SoftwareTemplate() { }
    public static SoftwareTemplate Download() {
        return new SoftwareTemplate() {
            name = "download",
            principalType = SoftwareEffect.Type.download,
            effects = new List<SoftwareEffect>{
                new SoftwareEffect(){
                    type = SoftwareEffect.Type.download,
                    level = 1,
                    name = "download"
                }
            },
            maxCharges = 1,
            icon = null
        };
    }
    public NetworkAction ToNetworkAction(List<CyberNode> path, CyberNode target) {
        // float lifetime = effect.type switch {
        //     SoftwareEffect.Type.compromise => 10f,
        //     SoftwareEffect.Type.download => 10f,
        //     SoftwareEffect.Type.scan => 3f,
        //     // SoftwareEffect.Type.unlock => 5f,
        //     SoftwareEffect.Type.unlock => 6f,
        //     _ => 1f
        // };
        float lifetime = 2.5f;

        NetworkAction networkAction = new NetworkAction() {
            title = $"uploading {name}...",
            softwareTemplate = this,
            lifetime = lifetime,
            toNode = path[path.Count - 1],
            timerRate = 1f,
            payData = target.payData,
            path = path,
        };

        return networkAction;
    }
}