using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Rendering;
public class MaterialController {
    public Renderer renderer;
    public List<Renderer> childRenderers;
    public GameObject gameObject;
    public NeoCharacterCamera camera;
    public TagSystemData tagSystemData;
    public float timer;
    public bool disableBecauseInterloper;
    public bool disableBecauseAbove;
    public MaterialController(GameObject gameObject, NeoCharacterCamera camera) {
        this.camera = camera;
        this.gameObject = gameObject;
        this.renderer = gameObject.GetComponent<Renderer>();
        this.tagSystemData = Toolbox.GetTagData(gameObject);
        this.childRenderers = new List<Renderer>(gameObject.GetComponentsInChildren<Renderer>());
        childRenderers.Remove(renderer);
    }
    public void InterloperStart() {
        // Debug.Log($"{gameObject} {renderer} interloper start");
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
        // handle transparent objects differently
        if (renderer.shadowCastingMode != ShadowCastingMode.ShadowsOnly) {
            renderer.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
            foreach (Renderer renderer in childRenderers) {
                renderer.enabled = false;
            }
        }
    }
    public void MakeApparent() {
        if (renderer.shadowCastingMode != ShadowCastingMode.On) {
            renderer.shadowCastingMode = ShadowCastingMode.On;
            foreach (Renderer renderer in childRenderers) {
                renderer.enabled = true;
            }
        }
    }
    public void Update() {
        if (renderer == null)
            return;
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

    public bool active() {
        return disableBecauseInterloper || disableBecauseAbove;
    }
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
            // Debug.Log($"materialcontroller initialize {key}:{controller.renderer}");
            if (controller.renderer == null) {
                controller = null;
            }
            controllers[key] = controller;
            return controller;
        }
    }
}
public class ClearSighter : MonoBehaviour {
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
        Vector3 mypos = transform.position + new Vector3(0, 1f, 0);
        float distance = Vector3.Distance(myCamera.transform.position, mypos);
        Vector3 direction = (mypos - myCamera.transform.position).normalized;

        Vector3 startPosition = myCamera.transform.position;
        // Debug.DrawRay(startPosition, distance * direction, Color.red, 1f);
        foreach (RaycastHit hit in Physics.RaycastAll(startPosition, direction, distance).OrderBy(x => x.distance)) {
            // Debug.Log(hit.collider.gameObject);
            if (hit.collider.transform.IsChildOf(transform)) {
                continue;
            }
            if (Toolbox.GetTagData(hit.collider.gameObject).dontHideInterloper) {
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
