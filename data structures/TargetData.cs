using UnityEngine;
public class TargetData {
    public enum TargetType { none, direction, objectLock, interactiveHighlight }
    public TargetType type;
    public Vector3 position;
    public Vector2 screenPosition;
    public HighlightableTargetData highlightableTargetData;
    public static TargetData none = new TargetData();
}