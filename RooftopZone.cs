using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class RooftopZone : MonoBehaviour {
    public string idn;
    public List<Collider> playerDetectionColliders;
    public List<Collider> geometryInclusionColliders;

    public bool ContainsPlayerPoint(Vector3 point) {
        return playerDetectionColliders.Any(collider => collider.bounds.Contains(point));
    }
    public bool ContainsGeometry(Vector3 point) {
        return geometryInclusionColliders.Any(collider => collider.bounds.Contains(point));
    }
}
