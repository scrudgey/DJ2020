using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Rendering;
public class MaterialController {
    public MeshRenderer renderer;
    public GameObject gameObject;
    public NeoCharacterCamera camera;
    public float timer;
    public bool disableBecauseInterloper;
    public bool disableBecauseAbove;
    public MaterialController(GameObject gameObject, NeoCharacterCamera camera) {
        this.camera = camera;
        this.gameObject = gameObject;
        this.renderer = gameObject.GetComponent<MeshRenderer>();
    }
    public void InterloperStart() {
        timer = 1f;
    }
    public void CeilingCheck(Collider collider, Vector3 position) {
        if (collider.bounds.center.y < position.y) {
            disableBecauseAbove = false;
            return;
        }
        Vector3 otherFloor = collider.bounds.center - new Vector3(0f, collider.bounds.extents.y, 0f);
        Vector3 direction = otherFloor - position;
        if (direction.y > 2.0f) {
            disableBecauseAbove = true;
        } else {
            disableBecauseAbove = false;
        }
    }
    public void MakeTransparent() {
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

        if (active() && camera.state == CameraState.normal) {
            MakeTransparent();
        } else {
            MakeApparent();
        }
    }

    public bool active() { return disableBecauseInterloper || disableBecauseAbove; }
}
public class MaterialControllerCache {
    public NeoCharacterCamera camera;
    public Dictionary<GameObject, MaterialController> controllers = new Dictionary<GameObject, MaterialController>();
    public MaterialControllerCache(NeoCharacterCamera camera) {
        this.camera = camera;
    }
    public MaterialController get(GameObject key) {
        if (controllers.ContainsKey(key)) {
            return controllers[key];
        } else {
            MaterialController controller = new MaterialController(key, camera);
            if (controller.renderer == null) {
                // Debug.Log($"{key} null controller");
                controller = null;
            }
            controllers[key] = controller;
            return controller;
        }
    }
}
public class ClearSighter : MonoBehaviour {
    static List<string> forbiddenTags = new List<string> { "glass", "donthide" };

    public MaterialControllerCache controllers;
    public NeoCharacterCamera myCamera;
    Transform myTransform;
    void Start() {
        controllers = new MaterialControllerCache(GameObject.FindObjectOfType<NeoCharacterCamera>());
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
            if (forbiddenTags.Contains(hit.collider.tag)) {
                continue;
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
