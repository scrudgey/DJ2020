using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class SoftwareCondition {
    public enum Type { nodeType, unlocked, nodeKnown, manualHack, locked, uncompromised, nodeUnknown, fileUnknown, edgesUnknown, anyUnknown }
    public Type type;
    public CyberNodeType matchType;
    public SoftwareCondition(Type type) {
        this.type = type;
        matchType = CyberNodeType.normal;
    }
    public SoftwareCondition(Type type, CyberNodeType nodeType) {
        this.type = type;
        this.matchType = nodeType;
    }
    public bool Evaluate(CyberNode target, CyberNode origin, List<CyberNode> path) {
        switch (type) {
            case Type.unlocked:
                return target.lockLevel == 0;
            case Type.nodeType:
                return target.type == matchType;
            case Type.nodeKnown:
                return target.visibility > NodeVisibility.mystery;
            case Type.manualHack:
                // Debug.Log($"origin: {origin} path: {path.Count}");
                // Debug.Log($"{origin.idn}");
                foreach (CyberNode node in path) {
                    Debug.Log($"path: {node.idn}");
                }
                return origin?.idn == "cyberdeck" && path.Count <= 2;
            case Type.locked:
                return target.lockLevel > 0;
            case Type.uncompromised:
                return !target.compromised;
            case Type.nodeUnknown:
                return target.visibility <= NodeVisibility.mystery;
            case Type.fileUnknown:
                return !target.datafileVisibility;
            case Type.anyUnknown:
                return target.visibility <= NodeVisibility.mystery || !GameManager.I.gameData.levelState.delta.cyberGraph.AllEdgesVisible(target.idn);
            case Type.edgesUnknown:
                return !GameManager.I.gameData.levelState.delta.cyberGraph.AllEdgesVisible(target.idn);
        }
        return true;
    }

    public string DescriptionStringWithColor(CyberNode target, CyberNode origin, List<CyberNode> path) {
        bool enabled = Evaluate(target, origin, path);

        string redColor = "#ff4757";
        string greenColor = "#2ed573";

        string description = DescriptionString();

        string color = enabled ? greenColor : redColor;

        return $"<color={color}>{description}</color>";
    }

    public string DescriptionString() {
        string description = type switch {
            Type.unlocked => "node is unlocked",
            Type.nodeType => $"node is {matchType}",
            Type.nodeKnown => "node is scanned",
            Type.manualHack => "using cyberdeck",
            Type.locked => "node is locked",
            Type.uncompromised => "node is not compromised",
            Type.nodeUnknown => "node type is unknown",
            Type.fileUnknown => "file type is unknown",
            Type.edgesUnknown => "neighbors not discovered",
            Type.anyUnknown => "node is not fully scanned"
        };
        return $"{description}";
    }

    public int Cost() {
        return -1;
    }

    // required for serialization?
    public SoftwareCondition() { }
}