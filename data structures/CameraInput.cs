using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Easings;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.PostProcessing;

public struct CameraInput {
    public enum RotateInput { none, left, right }
    public float deltaTime;
    public Vector3 wallNormal;
    public Vector2 lastWallInput;
    public bool crouchHeld;
    public Vector3 playerPosition;
    public CharacterState characterState;
    public CameraState state;
    public CursorData targetData;
    public Vector3 playerDirection;
    public PopoutParity popoutParity;
    public Quaternion aimCameraRotation;
    public Vector3 targetPosition;
    public Transform targetTransform;
    public bool atLeftEdge;
    public bool atRightEdge;
    public AttackSurface currentAttackSurface;
    public bool ignoreAttractor;
}