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
    public CharacterState state;
    public CursorData targetData;
    public Vector3 playerDirection;
    public Vector3 playerLookDirection;
    public PopoutParity popoutParity;
}