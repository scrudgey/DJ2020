using UnityEngine;
public class TargetData {
    public enum TargetType { none, direction, objectLock }
    public TargetType type;
    public Vector3 position;
    public Vector2 screenPosition;
    public static TargetData none = new TargetData();
}