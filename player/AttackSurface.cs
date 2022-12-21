using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AttackSurface : MonoBehaviour {
    public Camera attackCam;
    public RenderTexture renderTexture;
    public Transform mainCameraPosition;
    public void Start() {
        renderTexture = new RenderTexture(640, 480, 16, RenderTextureFormat.Default);
        attackCam.targetTexture = renderTexture;
        attackCam.enabled = false;
    }
    public void EnableAttackSurface() {
        attackCam.enabled = true;
    }
    public void DisableAttackSurface() {
        attackCam.enabled = false;
    }
}