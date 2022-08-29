using UnityEngine;

public class SpaceTimePoint {
    public float time;
    public Vector3 position;
    public SpaceTimePoint(Vector3 position) {
        this.position = position;
        this.time = Time.time;
    }
}