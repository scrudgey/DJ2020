[System.Serializable]
public class SoftwareCondition {
    public enum Type { nodeType, lockLevel, hops, nodevisibilty }
    public Type type;
    public CyberNodeType matchType;
    public NodeVisibility nodeVisibilityGreaterThan;
    public int magnitude;
    public bool Evaluate(CyberNode target) {
        switch (type) {
            case Type.lockLevel:
                return target.lockLevel <= magnitude;
            case Type.nodeType:
                return target.type == matchType;
            case Type.nodevisibilty:
                return target.visibility > nodeVisibilityGreaterThan;
        }
        return true;
    }

    public string DescriptionString(CyberNode target) {
        bool enabled = Evaluate(target);

        string redColor = "#ff4757";
        string greenColor = "#2ed573";

        string description = type switch {
            Type.lockLevel => "node is unlocked",
            Type.nodeType => $"node is {matchType}",
            Type.nodevisibilty => "node is scanned"
        };

        string color = enabled ? greenColor : redColor;

        return $"<color={color}>{description}</color>";

    }

    // required for serialization?
    public SoftwareCondition() { }
}