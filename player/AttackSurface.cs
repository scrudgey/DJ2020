using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using cakeslice;
using UnityEngine;
public class AttackSurface : MonoBehaviour {
    public string niceName;
    public Camera attackCam;
    [HideInInspector]
    public RenderTexture renderTexture;
    public Transform mainCameraPosition;
    public Outline outline;
    public Transform attackElementRoot;
    public void Start() {
        renderTexture = new RenderTexture(1250, 750, 16, RenderTextureFormat.Default);
        attackCam.targetTexture = renderTexture;
        attackCam.enabled = false;
        DisableOutline();
    }
    public void EnableAttackSurface() {
        attackCam.enabled = true;
    }
    public void DisableAttackSurface() {
        attackCam.enabled = false;
    }

    public void EnableOutline() {
        if (outline != null) {
            outline.enabled = true;
        }
    }
    public void DisableOutline() {
        if (outline != null) {
            outline.enabled = false;
        }
    }
}