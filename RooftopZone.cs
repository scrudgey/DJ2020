using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class RooftopZone : MonoBehaviour {
    public string idn;
    public List<Collider> colliders;

    public bool ContainsPoint(Vector3 point) {
        return colliders.Any(collider => collider.bounds.Contains(point));
    }
}
