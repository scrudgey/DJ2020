using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class ClearSighter : MonoBehaviour {
    public static Dictionary<GameObject, MeshRenderer> renderers = new Dictionary<GameObject, MeshRenderer>();
    // public static Dictionary<Material, Material> transparentMaterials = new Dictionary<Material, Material>();
    public static Dictionary<GameObject, TransparentMesh> transparentMeshes = new Dictionary<GameObject, TransparentMesh>();
    // public Camera myCamera;
    public NeoCharacterCamera myCamera;
    Transform myTransform;
    public bool active;
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
    // Start is called before the first frame update
    void Start() {
        myTransform = transform;
    }
    public void UpdateWithInput(CameraInput input) {
        active = input.state != CameraInput.CameraState.wallPress;
    }

    // Update is called once per frame
    void Update() {

        // colliders above me
        Collider[] others = Physics.OverlapSphere(transform.position, 20f);
        foreach (Collider collider in others) {
            if (collider.transform.IsChildOf(myTransform))
                continue;
            MeshRenderer renderer = Renderer(collider.gameObject);
            if (renderer == null)
                continue;
            if (!active) {
                // renderer.enabled = true;
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                continue;
            }

            if (collider.bounds.center.y < myTransform.position.y) {
                // renderer.enabled = true;
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;

                continue;
            }

            Vector3 otherFloor = collider.bounds.center - new Vector3(0f, collider.bounds.extents.y, 0f);
            Vector3 direction = otherFloor - myTransform.position;

            if (direction.y > 1.0f) {
                // renderer.enabled = false;
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
                // Debug.DrawRay(myTransform.position, direction, Color.white, 0.0f, true);
            } else {
                // renderer.enabled = true;
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;

            }
        }

        // collider between me and the camera
        // Vector3 gazePosition = transform.position + new Vector3(0f, 2f, 0f);
        float distance = Vector3.Distance(myCamera.transform.position, transform.position) + 1f;
        // Debug.DrawRay(myCamera.transform.position, distance * myCamera.transform.forward, Color.red, 0.1f);
        foreach (RaycastHit hit in Physics.RaycastAll(myCamera.transform.position, myCamera.transform.forward, distance).OrderBy(x => x.distance)) {
            if (hit.collider.transform.IsChildOf(transform)) {
                break;
            }
            // MeshRenderer mesh = Renderer(hit.collider.gameObject);
            // if (mesh.shadowCastingMode == UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly)
            //     continue;
            TransparentMesh transparent = TransparentMaterial(hit.collider.gameObject);
            if (transparent != null) {
                // mesh.material = transparent;
                transparent.timer = 1f;
            }
        }
    }
}
