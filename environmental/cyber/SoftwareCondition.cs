using System.Collections.Generic;

[System.Serializable]
public class SoftwareCondition {
    public enum Type { nodeType, unlocked, nodeKnown, manualHack, locked, uncompromised, nodeUnknown, fileUnknown, edgesUnknown }
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
                return origin?.idn == "cyberdeck";
            case Type.locked:
                return target.lockLevel > 0;
            case Type.uncompromised:
                return !target.compromised;
            case Type.nodeUnknown:
                return target.visibility <= NodeVisibility.mystery;
            case Type.fileUnknown:
                return !target.datafileVisibility;
            case Type.edgesUnknown:
                return true;
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
            Type.edgesUnknown => "neighbors not discovered"
        };
        return $"{description}";
    }

    public int Cost() {
        return -1;
    }

    // required for serialization?
    public SoftwareCondition() { }
}