using UnityEngine;

public class CursorData {
    public enum TargetType { none, direction, objectLock, interactiveHighlight }
    public TargetType type;
    public Vector3 worldPosition = Vector3.zero;
    public Vector3 groundPosition = Vector3.zero;
    public Vector2 screenPosition;
    public Vector2 screenPositionNormalized;
    public Vector2 screenPixelDimension;
    public InteractorTargetData highlightableTargetData;
    public AttackSurface attackSurface;
    public Collider targetCollider;
    public Vector2 mousePosition;
    public static CursorData none = new CursorData();
}