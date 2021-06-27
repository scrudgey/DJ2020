using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearSighter : MonoBehaviour {
    public static Dictionary<GameObject, MeshRenderer> renderers = new Dictionary<GameObject, MeshRenderer>();
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
    // Start is called before the first frame update
    void Start() {
        myTransform = transform;
    }
    public void UpdateWithInput(CameraInput input) {
        active = input.state != CameraInput.CameraState.wallPress;
    }

    // Update is called once per frame
    void Update() {
        Collider[] others = Physics.OverlapSphere(transform.position, 20f);
        foreach (Collider collider in others) {
            MeshRenderer renderer = Renderer(collider.gameObject);
            if (renderer == null)
                continue;
            if (!active) {
                renderer.enabled = true;
                continue;
            }

            if (collider.bounds.center.y < myTransform.position.y) {
                renderer.enabled = true;
                continue;
            }

            Vector3 otherFloor = collider.bounds.center - new Vector3(0f, collider.bounds.extents.y, 0f);
            Vector3 direction = otherFloor - myTransform.position;

            if (direction.y > 1.0f) {
                renderer.enabled = false;
                // Debug.DrawRay(myTransform.position, direction, Color.white, 0.0f, true);
            } else {
                renderer.enabled = true;
            }
        }
    }
}
