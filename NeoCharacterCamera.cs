using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public struct CameraInput {
    public enum RotateInput { none, left, right }
    public float deltaTime;
    public RotateInput rotation;
}
public class NeoCharacterCamera : MonoBehaviour {
    [Header("Framing")]
    public Camera Camera;
    public Vector2 FollowPointFraming = new Vector2(0f, 0f);
    public float FollowingSharpness = 10000f;

    [Header("Distance")]
    public float DefaultDistance = 6f;
    public float MinDistance = 0f;
    public float MaxDistance = 10f;
    public float DistanceMovementSpeed = 5f;
    public float DistanceMovementSharpness = 10f;

    [Header("Rotation")]
    public float RotationSpeed = 1f;
    public float RotationSharpness = 10000f;
    Quaternion targetRotation = Quaternion.identity;


    [Header("Obstruction")]
    public float ObstructionCheckRadius = 0.2f;
    public LayerMask ObstructionLayers = -1;
    public float ObstructionSharpness = 10000f;
    public List<Collider> IgnoredColliders = new List<Collider>();

    public Transform Transform { get; private set; }
    public Transform FollowTransform { get; private set; }

    public Vector3 PlanarDirection { get; set; }
    public float TargetDistance { get; set; }

    private bool _distanceIsObstructed;
    private float _currentDistance;
    private float _targetVerticalAngle;
    private RaycastHit _obstructionHit;
    private int _obstructionCount;
    private RaycastHit[] _obstructions = new RaycastHit[MaxObstructions];
    private float _obstructionTime;
    private Vector3 _currentFollowPosition;

    private const int MaxObstructions = 32;

    void OnValidate() {
        DefaultDistance = Mathf.Clamp(DefaultDistance, MinDistance, MaxDistance);
        // DefaultVerticalAngle = Mathf.Clamp(DefaultVerticalAngle, MinVerticalAngle, MaxVerticalAngle);
    }

    void Awake() {
        Transform = this.transform;

        _currentDistance = DefaultDistance;
        TargetDistance = _currentDistance;

        _targetVerticalAngle = 0f;

        PlanarDirection = Vector3.forward;
    }

    // Set the transform that the camera will orbit around
    public void SetFollowTransform(Transform t) {
        FollowTransform = t;
        PlanarDirection = Quaternion.Euler(0, -45, 0) * FollowTransform.forward;
        _currentFollowPosition = FollowTransform.position;

        targetRotation = Quaternion.identity;
    }

    public void UpdateWithInput(CameraInput input) {
        if (FollowTransform) {

            // Process rotation input
            float rotationInput = 0f;
            switch (input.rotation) {
                case CameraInput.RotateInput.left:
                    rotationInput = 90f;
                    break;
                case CameraInput.RotateInput.right:
                    rotationInput = -90f;
                    break;
                default:
                case CameraInput.RotateInput.none:
                    break;
            }
            Quaternion rotationFromInput = Quaternion.Euler(FollowTransform.up * rotationInput);// * RotationSpeed));
            PlanarDirection = rotationFromInput * PlanarDirection;
            PlanarDirection = Vector3.Cross(FollowTransform.up, Vector3.Cross(PlanarDirection, FollowTransform.up));
            Quaternion planarRot = Quaternion.LookRotation(PlanarDirection, FollowTransform.up);

            Quaternion verticalRot = Quaternion.Euler(33f, 0, 0);
            targetRotation = Quaternion.Slerp(targetRotation, planarRot * verticalRot, 1f - Mathf.Exp(-RotationSharpness * input.deltaTime));

            // Apply rotation
            Transform.rotation = targetRotation;

            // Process distance input
            // if (_distanceIsObstructed) {//&& Mathf.Abs(zoomInput) > 0f) {
            //     TargetDistance = _currentDistance;
            // }
            // TargetDistance += zoomInput * DistanceMovementSpeed;
            TargetDistance = Mathf.Clamp(TargetDistance, MinDistance, MaxDistance);

            // Find the smoothed follow position
            _currentFollowPosition = Vector3.Lerp(_currentFollowPosition, FollowTransform.position, 1f - Mathf.Exp(-FollowingSharpness * input.deltaTime));

            // Find the smoothed camera orbit position
            Vector3 targetPosition = _currentFollowPosition - ((targetRotation * Vector3.forward) * _currentDistance);

            // Handle framing
            targetPosition += Transform.right * FollowPointFraming.x;
            targetPosition += Transform.up * FollowPointFraming.y;

            // Apply position
            Transform.position = targetPosition;
        }
    }
}