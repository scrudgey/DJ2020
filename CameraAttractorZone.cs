using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraAttractorZone : MonoBehaviour {
    public SphereCollider sphereCollider;
    public bool useInnerFocus;
    public float innerFocusRadius;
    public float innerFocusOrthographicSize;
    public float movementSharpness;
}
