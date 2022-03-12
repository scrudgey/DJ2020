using UnityEngine;

public class TargetData2 {
    public enum TargetType { none, direction, objectLock, interactiveHighlight }
    public TargetType type;
    public Vector3 position = Vector3.zero;
    public Vector2 screenPosition;
    public HighlightableTargetData highlightableTargetData;
    public Ray clickRay;
    public static TargetData2 none = new TargetData2();
    public Vector3 targetPointFromRay(Vector3 origin) {
        // find the intersection between the ray and a plane whose normal is the player's up, and height is the gun height
        float distance = 0;
        Vector3 targetPoint = Vector3.zero;
        Plane plane = new Plane(Vector3.up, origin);
        if (plane.Raycast(clickRay, out distance)) {
            return clickRay.GetPoint(distance);
        } else return origin;
    }
}