using UnityEngine;

public class CursorData {
    public enum TargetType { none, direction, objectLock, interactiveHighlight }
    public TargetType type;
    public Vector3 worldPosition = Vector3.zero;
    public Vector2 screenPosition;
    public Vector2 screenPositionNormalized;
    public HighlightableTargetData highlightableTargetData;
    public static CursorData none = new CursorData();
    public Collider targetCollider;
    public Vector2 mousePosition;
}