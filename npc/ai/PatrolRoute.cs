using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolRoute : MonoBehaviour {
    public enum PatrolRouteType { loop, pingPong }
    public Transform[] points;
    public PatrolRouteType type;


#if UNITY_EDITOR
    private void OnDrawGizmos() {
        for (int i = 0; i < points.Length; i++) {
            int j = i + 1;
            if (j > points.Length - 1) {
                if (type == PatrolRouteType.loop) {
                    j = 0;
                } else {
                    return;
                }
            }
            Gizmos.DrawLine(points[i].position, points[j].position);
        }
    }
#endif
}
