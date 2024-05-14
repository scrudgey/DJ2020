using System.Collections.Generic;

[System.Serializable]
public class SoftwareCondition {
    public enum Type { nodeType, lockLevel, hops, nodevisibilty, manualHack }
    public Type type;
    public CyberNodeType matchType;
    public NodeVisibility nodeVisibilityGreaterThan;
    public int magnitude;
    public bool Evaluate(CyberNode target, CyberNode origin, List<CyberNode> path) {
        switch (type) {
            case Type.lockLevel:
                return target.lockLevel <= magnitude;
            case Type.nodeType:
                return target.type == matchType;
            case Type.nodevisibilty:
                return target.visibility > nodeVisibilityGreaterThan;
            case Type.manualHack:
                return origin?.idn == "localhost";
            case Type.hops:
                return path.Count <= magnitude;
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
            Type.lockLevel => "node is unlocked",
            Type.nodeType => $"node is {matchType}",
            Type.nodevisibilty => "node is scanned",
            Type.manualHack => "using cyberdeck",
            Type.hops => $"fewer than {magnitude} hops"
        };
        return $"{description}";
    }

    // required for serialization?
    public SoftwareCondition() { }
}