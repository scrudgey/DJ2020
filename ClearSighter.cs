using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class ClearSighter : MonoBehaviour {
    public static Dictionary<GameObject, MeshRenderer> renderers = new Dictionary<GameObject, MeshRenderer>();
    public static Dictionary<GameObject, TransparentMesh> transparentMeshes = new Dictionary<GameObject, TransparentMesh>();
    public NeoCharacterCamera myCamera;
    Transform myTransform;
    public static MeshRenderer Renderer(GameObject key) {
        if (renderers.ContainsKey(key)) {
            return renderers[key];
        } else {
            MeshRenderer renderer = key.GetComponentInChildren<MeshRenderer>();
            renderers[key] = renderer;
            return renderer;
        }
    }
    public static TransparentMesh TransparentMaterial(GameObject inMaterial) {
        if (transparentMeshes.ContainsKey(inMaterial)) {
            return transparentMeshes[inMaterial];
        } else {
            TransparentMesh transparent = inMaterial.AddComponent<TransparentMesh>();
            transparentMeshes[inMaterial] = transparent;
            return transparent;
        }
    }
    void Start() {
        myTransform = transform;
    }

    void Update() {
        // colliders above me
        Collider[] others = Physics.OverlapSphere(transform.position, 20f);
        foreach (Collider collider in others) {
            if (collider.transform.IsChildOf(myTransform))
                continue;
            MeshRenderer renderer = Renderer(collider.gameObject);
            if (renderer == null)
                continue;
            if (myCamera.state == CameraState.wallPress) {
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                continue;
            }

            if (collider.bounds.center.y < myTransform.position.y) {
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                continue;
            }

            Vector3 otherFloor = collider.bounds.center - new Vector3(0f, collider.bounds.extents.y, 0f);
            Vector3 direction = otherFloor - myTransform.position;
            if (direction.y > 2.0f) {
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
            } else {
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            }
        }

        // collider between me and the camera
        float distance = Vector3.Distance(myCamera.transform.position, transform.position) + 1f;
        foreach (RaycastHit hit in Physics.RaycastAll(myCamera.transform.position, myCamera.transform.forward, distance).OrderBy(x => x.distance)) {
            if (hit.collider.transform.IsChildOf(transform)) {
                break;
            }
            TransparentMesh transparent = TransparentMaterial(hit.collider.gameObject);
            if (transparent != null) {
                if (myCamera.state == CameraState.wallPress) {
                    transparent.timer = 0f;
                } else {
                    transparent.timer = 1f;
                }
            }
        }
    }
}
