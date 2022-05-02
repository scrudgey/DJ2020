using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolRoute : MonoBehaviour {
    public Transform[] points;


#if UNITY_EDITOR
    private void OnDrawGizmos() {
        for (int i = 0; i < points.Length; i++) {
            int j = i + 1;
            if (j > points.Length - 1) { j = 0; }
            Gizmos.DrawLine(points[i].position, points[j].position);
        }
    }
#endif
}
