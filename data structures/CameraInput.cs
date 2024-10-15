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
    public bool crouchHeld;
    public CharacterState characterState;
    public CameraState cameraState;
    public CursorData targetData;


    public Quaternion targetRotation;
    public Vector3 targetPosition;
    public Vector3 cullingTargetPosition;
    public Vector3 playerDirection;
    public Transform targetTransform;

    public float orthographicSize;
    public bool snapToOrthographicSize;
    public bool snapTo;


    [Header("wallpress")]
    public bool atLeftEdge;
    public bool atRightEdge;
    public Vector3 wallNormal;
    public Vector2 lastWallInput;
    public PopoutParity popoutParity;

    public AttackSurface currentAttackSurface;
    public bool ignoreAttractor;

    public static CameraInput None() {
        return new CameraInput();
    }
}