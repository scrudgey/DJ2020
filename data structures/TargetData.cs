using UnityEngine;
public class TargetData {
    public enum TargetType { none, direction, obj }
    public TargetType type;
    public Vector3 position;
    public static TargetData none = new TargetData();
}