using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
public class MaterialController {
    // public Renderer renderer;
    enum State { opaque, transparent, fadeOut, fadeIn }
    State state;
    public List<Renderer> childRenderers;
    public GameObject gameObject;
    public Collider collider;
    public CharacterCamera camera;
    public TagSystemData tagSystemData;
    public float timer;
    public bool disableBecauseInterloper;
    public bool disableBecauseAbove;
    public float ceilingHeight = 1.5f;
    Material normalMaterial;
    Material interloperMaterial;
    public float targetAlpha;
    Dictionary<Renderer, ShadowCastingMode> initialShadowCastingMode = new Dictionary<Renderer, ShadowCastingMode>();
    public MaterialController(GameObject gameObject, CharacterCamera camera) {
        this.camera = camera;
        this.gameObject = gameObject;
        this.tagSystemData = Toolbox.GetTagData(gameObject);
        this.childRenderers = new List<Renderer>(gameObject.GetComponentsInChildren<Renderer>()).Where(x => !(x is SpriteRenderer)).ToList();
        // this.childRenderers = new List<Renderer>(gameObject.GetComponentsInChildren<Renderer>()).ToList();
        this.collider = gameObject.GetComponentInChildren<Collider>();
        foreach (Renderer renderer in childRenderers) {
            initialShadowCastingMode[renderer] = renderer.shadowCastingMode;
            if (renderer.material != null)
                normalMaterial = renderer.material;
        }
        if (normalMaterial != null) {
            interloperMaterial = new Material(normalMaterial);
            interloperMaterial.shader = Resources.Load("Scripts/shaders/Interloper") as Shader;
        }
    }
    public void InterloperStart() {
        timer = 0.1f;
    }
    public void CeilingCheck(Collider collider, Vector3 position) {
        if (collider.bounds.center.y < position.y + 0.05f) {
            disableBecauseAbove = false;
            return;
        }
        Vector3 otherFloor = collider.bounds.center - new Vector3(0f, collider.bounds.extents.y, 0f);
        Vector3 direction = otherFloor - position;
        if (direction.y > ceilingHeight) {
            // TODO: don't disable when character is midair.
            disableBecauseAbove = true;
        } else {
            disableBecauseAbove = false;
        }
    }
    public void MakeFadeOut() {
        if (state == State.transparent || state == State.fadeOut)
            return;
        state = State.fadeOut;
        foreach (Renderer renderer in childRenderers) {
            if (renderer == null)
                continue;
            renderer.material = interloperMaterial;
            renderer.material.SetFloat("_TargetAlpha", 1);
            targetAlpha = 1;
            if (renderer is SpriteRenderer) {
                renderer.enabled = false;
            }
        }
    }
    public void MakeFadeIn() {
        if (state == State.opaque || state == State.fadeIn)
            return;
        state = State.fadeIn;
        foreach (Renderer renderer in childRenderers) {
            renderer.enabled = true;
        }
        // if (renderer.shadowCastingMode != ShadowCastingMode.On) {
        //     // renderer.shadowCastingMode = ShadowCastingMode.On;
        //     renderer.shadowCastingMode = initialShadowCastingMode[renderer];
        //     // renderer.enabled = true;
        // }
    }
    public void MakeOpaque() {
        foreach (Renderer renderer in childRenderers) {
            if (renderer == null)
                return;
            if (renderer.shadowCastingMode != ShadowCastingMode.On) {
                // renderer.shadowCastingMode = ShadowCastingMode.On;
                renderer.shadowCastingMode = initialShadowCastingMode[renderer];
                // renderer.enabled = true;
            }
            renderer.material = normalMaterial;
        }
    }
    public void Update(float offAxisLength) {
        if (childRenderers.Count == 0)
            return;
        float minimumAlpha = 0.2f;
        if (disableBecauseAbove) {
            minimumAlpha = 0f;
            // foreach (Renderer renderer in childRenderers) {
            //     renderer.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
            // }
        } else {
            minimumAlpha = (1f * (offAxisLength / 2f));
            // foreach (Renderer renderer in childRenderers) {
            //     renderer.shadowCastingMode = ShadowCastingMode.On;
            // }
        }
        if (state == State.fadeIn) {
            if (targetAlpha < 1) {
                targetAlpha += Time.unscaledDeltaTime * 3f;
            } else {
                MakeOpaque();
            }

        } else if (state == State.fadeOut) {
            if (targetAlpha > minimumAlpha) {
                targetAlpha -= Time.unscaledDeltaTime * 3f;
                targetAlpha = Mathf.Max(targetAlpha, minimumAlpha);
            } else {
                targetAlpha = minimumAlpha;
            }
            foreach (Renderer renderer in childRenderers) {
                if (targetAlpha == 0) {
                    renderer.enabled = false;
                } else {
                    renderer.enabled = true;
                }
            }
        }

        targetAlpha = Mathf.Max(0f, targetAlpha);
        targetAlpha = Mathf.Min(1f, targetAlpha);

        if (state == State.fadeIn || state == State.fadeOut) {
            foreach (Renderer renderer in childRenderers) {
                if (renderer == null)
                    continue;
                renderer.material.SetFloat("_TargetAlpha", targetAlpha);
            }
        }

        // interloper logic
        if (timer > 0)
            timer -= Time.deltaTime;
        if (timer <= 0) {
            disableBecauseInterloper = false;
        } else {
            disableBecauseInterloper = true;
        }

        if (active() && (camera.state == CameraState.normal || camera.state == CameraState.attractor)) {
            MakeFadeOut();
        } else {
            MakeFadeIn();
        }
    }

    public bool active() {
        return (disableBecauseInterloper && !tagSystemData.bulletPassthrough && !tagSystemData.dontHideInterloper) || (disableBecauseAbove && !tagSystemData.dontHideAbove);
    }
}
public class MaterialControllerCache {
    public CharacterCamera camera;
    public Dictionary<GameObject, MaterialController> controllers = new Dictionary<GameObject, MaterialController>();
    public MaterialControllerCache(CharacterCamera camera) {
        this.camera = camera;
    }
    public MaterialController get(GameObject key) {
        if (controllers.ContainsKey(key)) {
            return controllers[key];
        } else {
            MaterialController controller = new MaterialController(key, camera);
            if (controller.childRenderers.Count == 0) {
                controller = null;
            }
            controllers[key] = controller;
            return controller;
        }
    }
}
public class ClearSighter : MonoBehaviour {
    public MaterialControllerCache controllers;
    public CharacterCamera myCamera;
    public Shader normalShader;
    public Shader interloperShader;
    public Collider cylinderCollider;
    Transform myTransform;
    public List<MaterialController> interlopers = new List<MaterialController>();
    public Transform followTransform;
    void Start() {
        controllers = new MaterialControllerCache(GameObject.FindObjectOfType<CharacterCamera>());
        myTransform = transform;
        // InvokeRepeating("DoUpdate", 0f, 1f);
    }

    void LateUpdate() {
        if (followTransform == null)
            return;
        myTransform.position = followTransform.position;

        // TODO: invokeRepeating here
        // TODO: set cylinder height
        // TODO: set cylinder position

        Vector3 directionToCamera = (myCamera.transform.position - myTransform.position).normalized;
        myTransform.rotation = Quaternion.LookRotation(directionToCamera);

        // garbage collect
        HashSet<MaterialController> removeControllers = new HashSet<MaterialController>();

        // colliders above me
        Collider[] others = Physics.OverlapSphere(myTransform.position, 20f, LayerUtil.GetMask(Layer.def, Layer.obj, Layer.shell, Layer.bulletPassThrough));
        foreach (Collider collider in others) {
            if (collider == null || collider.gameObject == null)
                continue;
            if (collider.transform.IsChildOf(myTransform) || collider.transform.IsChildOf(followTransform))
                continue;
            if (collider.tag == "shell")
                continue;
            MaterialController controller = controllers.get(collider.gameObject);
            if (controller != null) {
                controller.CeilingCheck(collider, myTransform.position);
            } else {
                removeControllers.Add(controller);
            }
        }

        // interloper colliders
        foreach (MaterialController interloper in interlopers) {
            if (interloper == null || interloper.gameObject == null) {
                removeControllers.Add(interloper);
                continue;
            }
            Vector3 directionToInterloper = interloper.gameObject.transform.position - myTransform.position;
            if (Vector3.Dot(directionToCamera, directionToInterloper) > 0 && directionToInterloper.y > -0.03f)
                interloper.InterloperStart();
        }

        // update
        foreach (MaterialController controller in controllers.controllers.Values) {
            if (controller == null || controller.gameObject == null) {
                removeControllers.Add(controller);
                continue;
            }
            Vector3 directionToMesh = myCamera.transform.position - controller.collider.bounds.center;
            float axialDistance = Vector3.Dot(directionToMesh, directionToCamera);
            Vector3 offAxis = directionToMesh - (axialDistance * directionToCamera);
            float offAxisLength = offAxis.magnitude;
            controller.Update(offAxisLength);
        }
        // foreach (MaterialController key in removeControllers) {
        //     // controllers.controllers.Remove(key.gameObject);
        //     controllers.controllers.rem
        // }
    }

    void OnTriggerEnter(Collider other)
    => AddInterloper(other);

    void OnTriggerExit(Collider other)
        => RemoveInterloper(other);

    void AddInterloper(Collider other) {
        if (followTransform == null)
            return;
        // Debug.Log("onaddinterloper");
        if (other.transform.IsChildOf(myTransform.root) || other.transform.IsChildOf(followTransform)) {
            return;
        }
        MaterialController controller = controllers.get(other.gameObject);
        interlopers.Add(controller);
        RemoveNullInterlopers();
    }
    void RemoveInterloper(Collider other) {
        if (other.transform.IsChildOf(myTransform.root) || other.transform.IsChildOf(followTransform)) {
            // if (other.transform.IsChildOf(myTransform.root)) {
            return;
        }
        MaterialController controller = controllers.get(other.gameObject);
        interlopers.Remove(controller);
        RemoveNullInterlopers();
    }
    void RemoveNullInterlopers() {
        interlopers = interlopers
            .Where(f => f != null)
            .ToList();
    }
}
