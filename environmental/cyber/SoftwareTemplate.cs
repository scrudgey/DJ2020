using System.Collections.Generic;
using UnityEngine;

public class SoftwareTemplate {
    public string name;
    public SoftwareEffect.Type principalType;
    public List<SoftwareEffect> effects;
    public int maxCharges;
    public Sprite icon;
    public List<SoftwareCondition> conditions;

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
            fromPlayerNode = target.isManualHackerTarget
        };
        // else if (effect.type == SoftwareEffect.Type.download) {
        //     networkAction.title = $"downloading {node.payData.filename}...";
        //     networkAction.path = graph.GetPathToNearestDownloadPoint(node);
        //     if (node.isManualHackerTarget) {
        //         networkAction.fromPlayerNode = true;
        //     }
        // } 
        // else if (node.isManualHackerTarget) {
        //     networkAction.fromPlayerNode = true;
        // } else {
        //     networkAction.path.Add(node);
        //     networkAction.path.Add(graph.GetNearestCompromisedNode(node));
        // }

        // if (networkAction.path.Count > 1 && networkAction.path[networkAction.path.Count - 1].isManualHackerTarget) {
        //     networkAction.fromPlayerNode = true;
        // }

        return networkAction;
    }
}