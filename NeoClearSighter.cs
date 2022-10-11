using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;

public class NeoClearSighter : MonoBehaviour {
    public MaterialControllerCache controllers;
    public CharacterCamera myCamera;
    public Shader normalShader;
    public Shader interloperShader;
    public Collider cylinderCollider;
    public float floorHeight;
    Transform myTransform;
    public List<MaterialController> interlopers = new List<MaterialController>();
    List<(MaterialController, Vector3)> staticGeometry = new List<(MaterialController, Vector3)>();
    public Transform followTransform;
    private List<Collider> rooftopZones = new List<Collider>();

    Coroutine coroutine;
    bool inRooftopZone;
    void Start() {
        myTransform = transform;
        InitializeMaterialControllerCache();
        // InvokeRepeating("HandleStaticGeometry", 0f, 1f);
        rooftopZones = GameObject.FindObjectsOfType<RooftopZone>().Select(zone => zone.GetComponent<Collider>()).ToList();
        coroutine = StartCoroutine(RunJobRepeatedly());
    }
    IEnumerator RunJobRepeatedly() {
        while (true) {
            if (followTransform == null) {
                yield return null;
                continue;
            }
            myTransform.position = followTransform.position;
            Vector3 directionToCamera = -1f * myCamera.transform.forward;
            myTransform.rotation = Quaternion.LookRotation(directionToCamera);

            inRooftopZone = false;
            foreach (Collider zone in rooftopZones) {
                if (zone.bounds.Contains(myTransform.position)) {
                    inRooftopZone = true;
                }
            }
            // colliders above me
            Collider[] others = Physics.OverlapSphere(myTransform.position, 20f, LayerUtil.GetMask(Layer.obj, Layer.bulletPassThrough))//, Layer.shell))
                .GroupBy(collider => collider.transform.root)
                .Select(g => g.First())
                .Where(collider =>
                    collider != null &&
                    collider.gameObject != null &&
                    !collider.transform.IsChildOf(myTransform) &&
                    !collider.transform.IsChildOf(followTransform))
                // collider.tag != "shell")
                .ToArray();

            UpdateMaterialControllersJob x = new UpdateMaterialControllersJob(myTransform.position, directionToCamera, inRooftopZone, floorHeight, others, staticGeometry, controllers, interlopers);
            JobHandle jobHandle = x.Schedule();
            yield return null;
            while (!jobHandle.IsCompleted) {
                yield return null;
            }
        }
    }
    public struct UpdateMaterialControllersJob : IJob {
        Vector3 myPosition;
        Collider[] others;
        List<(MaterialController, Vector3)> staticGeometry;
        bool inRooftopZone;
        MaterialControllerCache controllers;
        float floorHeight;
        List<MaterialController> interlopers;
        Vector3 directionToCamera;
        public UpdateMaterialControllersJob(Vector3 myPosition,
                                            Vector3 directionToCamera,
                                            bool inRooftopZone,
                                            float floorHeight,
                                            Collider[] others,
                                            List<(MaterialController, Vector3)> staticGeometry,
                                            MaterialControllerCache controllers,
                                            List<MaterialController> interlopers) {
            this.myPosition = myPosition;
            this.others = others;
            this.staticGeometry = staticGeometry;
            this.inRooftopZone = inRooftopZone;
            this.controllers = controllers;
            this.floorHeight = floorHeight;
            this.interlopers = interlopers;
            this.directionToCamera = directionToCamera;
        }

        public void Execute() {
            // Vector3 myPosition = myTransform.position;

            foreach ((MaterialController controller, Vector3 position) in staticGeometry) {
                if (position.y > floorHeight)
                    continue;
                if (controller == null || controller.gameObject == null || controller.collider == null || !controller.gameObject.activeInHierarchy)
                    continue;
                if (inRooftopZone) {
                    controller.disableBecauseAbove = false;
                } else {
                    controller.CeilingCheck(myPosition);
                }
                controller.UpdateTargetAlpha(0);
                controller.Update();
            }

            foreach (Collider collider in others) {
                MaterialController controller = controllers.get(collider);
                if (controller != null) {
                    if (inRooftopZone) {
                        controller.disableBecauseAbove = false;
                    } else {
                        controller.CeilingCheck(myPosition);
                    }
                }
            }

            // interloper colliders
            // Debug.Log($"interlopers: {interlopers.Count}");
            foreach (MaterialController interloper in interlopers.Where(interloper =>
                                                                        interloper != null &&
                                                                        interloper.gameObject != null
                                                                        )) {
                Vector3 directionToInterloper = interloper.collider.bounds.center - myPosition;
                if (Vector3.Dot(directionToCamera, directionToInterloper) > 0 && directionToInterloper.y > -0.01f)
                    interloper.InterloperStart();

                Vector3 directionToMesh = interloper.collider.bounds.center - myPosition;
                float axialDistance = Vector3.Dot(directionToMesh, directionToCamera);
                Vector3 offAxis = directionToMesh - (axialDistance * directionToCamera);
                float offAxisLength = offAxis.magnitude;
                interloper.UpdateTargetAlpha(offAxisLength: offAxisLength);
                interloper.Update();
            }
            foreach (MaterialController controller in controllers.controllers.Values.Where(controller =>
                                                                                            controller != null)) {
                if (interlopers.Contains(controller))
                    continue;
                controller.UpdateTargetAlpha(0);
                controller.Update();
            }
        }
    }
    void InitializeMaterialControllerCache() {
        CharacterCamera cam = GameObject.FindObjectOfType<CharacterCamera>();
        controllers = new MaterialControllerCache(cam);
        List<Collider> allColliders = GameObject.FindObjectsOfType<Collider>(true)
            .Where(collider => !collider.isTrigger)
            .GroupBy(collider => collider.transform.root)
            .Select(g => g.First())
            .ToList();
        foreach (Collider collider in allColliders) {
            if (collider == null || collider.gameObject == null)
                continue;
            if (collider.transform.IsChildOf(myTransform))
                continue;
            MaterialController controller = controllers.get(collider);
            if (collider.gameObject.isStatic) {
                staticGeometry.Add((controller, collider.transform.position));
            }
        }
        Debug.Log($"[ClearSighter] initialized material controller cache with {controllers.controllers.Count}");
    }

    void HandleStaticGeometry() {
        Vector3 myPosition = myTransform.position;

        foreach ((MaterialController controller, Vector3 position) in staticGeometry.Where(x => x.Item2.y > floorHeight)) {
            if (controller == null || controller.gameObject == null || controller.collider == null || !controller.gameObject.activeInHierarchy)
                continue;
            if (inRooftopZone) {
                controller.disableBecauseAbove = false;
            } else {
                controller.CeilingCheck(myPosition);
            }
            controller.UpdateTargetAlpha(0);
            controller.Update();
        }
    }
    // void LateUpdate() {
    //     ProcessAllUpdates();
    // }
    void ProcessAllUpdates() {
        if (followTransform == null)
            return;
        myTransform.position = followTransform.position;

        inRooftopZone = false;
        foreach (Collider zone in rooftopZones) {
            if (zone.bounds.Contains(myTransform.position)) {
                inRooftopZone = true;
            }
        }

        Vector3 directionToCamera = -1f * myCamera.transform.forward;
        myTransform.rotation = Quaternion.LookRotation(directionToCamera);

        // colliders above me
        Collider[] others = Physics.OverlapSphere(myTransform.position, 20f, LayerUtil.GetMask(Layer.obj, Layer.bulletPassThrough))//, Layer.shell))
            .GroupBy(collider => collider.transform.root)
            .Select(g => g.First())
            .Where(collider =>
                collider != null &&
                collider.gameObject != null &&
                !collider.transform.IsChildOf(myTransform) &&
                !collider.transform.IsChildOf(followTransform))
            // collider.tag != "shell")
            .ToArray();
        foreach (Collider collider in others) {
            MaterialController controller = controllers.get(collider);
            if (controller != null) {
                // controller.CeilingCheck(myTransform.position);
                if (inRooftopZone) {
                    controller.disableBecauseAbove = false;
                } else {
                    controller.CeilingCheck(myTransform.position);
                }
            }
        }

        // interloper colliders
        // Debug.Log($"interlopers: {interlopers.Count}");
        foreach (MaterialController interloper in interlopers.Where(interloper =>
                                                                    interloper != null &&
                                                                    interloper.gameObject != null
                                                                    )) {
            Vector3 directionToInterloper = interloper.collider.bounds.center - myTransform.position;
            if (Vector3.Dot(directionToCamera, directionToInterloper) > 0 && directionToInterloper.y > -0.01f)
                interloper.InterloperStart();

            Vector3 directionToMesh = interloper.collider.bounds.center - myTransform.position;
            float axialDistance = Vector3.Dot(directionToMesh, directionToCamera);
            Vector3 offAxis = directionToMesh - (axialDistance * directionToCamera);
            float offAxisLength = offAxis.magnitude;
            interloper.UpdateTargetAlpha(offAxisLength: offAxisLength);
            interloper.Update();
        }
        foreach (MaterialController controller in controllers.controllers.Values.Where(controller =>
                                                                                        controller != null &&
                                                                                        !interlopers.Contains(controller))) {
            controller.UpdateTargetAlpha(0);
            controller.Update();
        }
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
        MaterialController controller = controllers.get(other);
        interlopers.Add(controller);
        RemoveNullInterlopers();
    }
    void RemoveInterloper(Collider other) {
        if (other.transform.IsChildOf(myTransform.root) || other.transform.IsChildOf(followTransform)) {
            return;
        }
        MaterialController controller = controllers.get(other);
        interlopers.Remove(controller);
        RemoveNullInterlopers();
    }
    void RemoveNullInterlopers() {
        interlopers = interlopers
            .Where(f => f != null)
            .ToList();
    }

    void OnDestroy() {
        StopCoroutine(coroutine);
    }
}
