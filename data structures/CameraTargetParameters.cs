using UnityEngine;
public record CameraTargetParameters {
    public float fieldOfView;
    public float deltaTime;
    public Quaternion rotation;
    public Quaternion snapToRotation;
    public bool orthographic;
    public float orthographicSize;
    public float targetDistance;
    public Vector3 targetPosition;
    public float followingSharpness;
    public float distanceMovementSharpness;
    public float rotationSharpness = 10f;
    public float minDistance;
}