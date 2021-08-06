using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Rendering;
public class MaterialController {
    public MeshRenderer renderer;
    public GameObject gameObject;
    public float timer;
    public bool disableBecauseInterloper;
    public bool disableBecauseAbove;
    public MaterialController(GameObject gameObject) {
        this.gameObject = gameObject;
        this.renderer = gameObject.GetComponent<MeshRenderer>();
    }
    public void InterloperStart() {
        timer = 1f;
    }
    public void CeilingCheck(Collider collider, Vector3 position) {
        if (collider.bounds.center.y < position.y) {
            // renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            disableBecauseAbove = false;
            return;
        }
        Vector3 otherFloor = collider.bounds.center - new Vector3(0f, collider.bounds.extents.y, 0f);
        Vector3 direction = otherFloor - position;
        if (direction.y > 2.0f) {
            disableBecauseAbove = true;
            // renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
        } else {
            disableBecauseAbove = false;
            // renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        }
    }
    public void MakeTransparent() {
        // StandardShaderUtils.ChangeRenderMode(renderer, StandardShaderUtils.BlendMode.Transparent);
        if (renderer.shadowCastingMode != ShadowCastingMode.ShadowsOnly)
            renderer.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
    }
    public void MakeApparent() {
        if (renderer.shadowCastingMode != ShadowCastingMode.On)
            renderer.shadowCastingMode = ShadowCastingMode.On;
    }
    public void Update() {
        // interloper logic
        if (timer > 0)
            timer -= Time.deltaTime;
        if (timer <= 0) {
            disableBecauseInterloper = false;
        } else {
            disableBecauseInterloper = true;
        }

        if (active()) {
            MakeTransparent();
        } else {
            MakeApparent();
        }
    }

    public bool active() { return disableBecauseInterloper || disableBecauseAbove; }

    // wallpress handling
    // if (myCamera.state == CameraState.wallPress) {
    //     renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
    //     continue;
    // }
}
public class MaterialControllerCache {
    public Dictionary<GameObject, MaterialController> controllers = new Dictionary<GameObject, MaterialController>();
    // public static Dictionary<GameObject, MeshRenderer> renderers = new Dictionary<GameObject, MeshRenderer>();
    public MaterialController get(GameObject key) {
        if (controllers.ContainsKey(key)) {
            return controllers[key];
        } else {
            MaterialController controller = new MaterialController(key);
            if (controller.renderer == null) {
                Debug.Log($"{key} null controller");
                controller = null;
            }
            controllers[key] = controller;
            return controller;
        }
    }
}
public class ClearSighter : MonoBehaviour {
    public MaterialControllerCache controllers = new MaterialControllerCache();
    public NeoCharacterCamera myCamera;
    Transform myTransform;
    void Start() {
        myTransform = transform;
    }

    void Update() {
        // colliders above me
        Collider[] others = Physics.OverlapSphere(myTransform.position, 20f);
        foreach (Collider collider in others) {
            if (collider.transform.IsChildOf(myTransform))
                continue;
            MaterialController controller = controllers.get(collider.gameObject);
            if (controller != null)
                controller.CeilingCheck(collider, myTransform.position);
        }


        // collider between me and the camera
        float distance = Vector3.Distance(myCamera.transform.position, transform.position) + 1f;
        foreach (RaycastHit hit in Physics.RaycastAll(myCamera.transform.position, myCamera.transform.forward, distance).OrderBy(x => x.distance)) {
            if (hit.collider.transform.IsChildOf(transform)) {
                break;
            }
            MaterialController controller = controllers.get(hit.collider.gameObject);
            if (controller != null) {
                controller.InterloperStart();
            }
        }

        // garbage collect
        HashSet<MaterialController> removeControllers = new HashSet<MaterialController>();
        foreach (MaterialController controller in controllers.controllers.Values) {
            if (controller == null)
                continue;
            controller.Update();
            if (!controller.active()) {
                removeControllers.Add(controller);
            }
        }
        foreach (MaterialController key in removeControllers) {
            controllers.controllers.Remove(key.gameObject);
        }
    }
}
