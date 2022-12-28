using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;

public class ClearSighter : MonoBehaviour {
    public MaterialControllerCache controllers;
    public CharacterCamera myCamera;
    public float floorHeight;
    Transform myTransform;
    public List<MaterialController> interlopers = new List<MaterialController>();
    Dictionary<MaterialController, Vector3> staticGeometry = new Dictionary<MaterialController, Vector3>();
    public Transform followTransform;
    private List<Collider> rooftopZones = new List<Collider>();
    Plane cullingPlane;
    Coroutine coroutine;
    bool inRooftopZone;
    Collider[] colliderHits;
    void Start() {
        colliderHits = new Collider[5000];
        myTransform = transform;
        InitializeMaterialControllerCache();
        InvokeRepeating("HandleStaticGeometry", 0f, 1f);
        rooftopZones = GameObject.FindObjectsOfType<RooftopZone>()
            .SelectMany(zone => zone.GetComponentsInChildren<Collider>())
            .ToList();
        coroutine = StartCoroutine(RunJobRepeatedly());
    }
    IEnumerator RunJobRepeatedly() {
        WaitForEndOfFrame waitForFrame = new WaitForEndOfFrame();
        while (true) {
            if (followTransform == null) {
                yield return null;
                continue;
            }
            int j = 0;
            int interloperCount = interlopers.Count;
            int allControllerCount = controllers.controllers.Count;

            myTransform.position = followTransform.position;

            inRooftopZone = false;
            foreach (Collider zone in rooftopZones) {
                if (zone.bounds.Contains(myTransform.position)) {
                    inRooftopZone = true;
                }
            }

            Vector3 directionToCamera = -1f * myCamera.transform.forward;
            myTransform.rotation = Quaternion.LookRotation(directionToCamera);

            // non-static colliders above me
            int numberHits = Physics.OverlapSphereNonAlloc(myTransform.position, 20f, colliderHits, LayerUtil.GetLayerMask(Layer.obj, Layer.bulletPassThrough, Layer.shell), QueryTriggerInteraction.Ignore);
            // Debug.Log($"ovelap hits: {numberHits}");
            for (int k = 0; k < numberHits; k++) {
                Collider collider = colliderHits[k];
                if (collider == null || collider.gameObject == null || collider.transform.IsChildOf(myTransform) || collider.transform.IsChildOf(followTransform))
                    continue;
                j += 1;
                if (j > 500) {
                    j = 0;
                    yield return waitForFrame;
                }
                MaterialController controller = controllers.get(collider);
                if (controller != null) {
                    if (inRooftopZone) {
                        controller.disableBecauseAbove = false;
                    } else {
                        controller.CeilingCheck(myTransform.position, cullingPlane, floorHeight);
                    }
                }
            }

            // interloper colliders
            // Debug.Log($"interlopers: {interlopers.Count}");
            for (int k = 0; k < interloperCount; k++) {
                // in case the collection was modified in the interim
                if (k >= interlopers.Count)
                    break;
                MaterialController interloper = interlopers[k];
                if (interloper == null || interloper.gameObject == null)
                    continue;
                interloper.updatedThisLoop = true;
                j += 1;
                if (j > 500) {
                    j = 0;
                    yield return waitForFrame;
                }
                Vector3 directionToInterloper = interloper.collider.bounds.center - myTransform.position;
                if (Vector3.Dot(directionToCamera, directionToInterloper) > 0 && directionToInterloper.y > 0.1f)
                    interloper.InterloperStart();

                Vector3 directionToMesh = interloper.collider.bounds.center - myTransform.position;
                float axialDistance = Vector3.Dot(directionToMesh, directionToCamera);
                Vector3 offAxis = directionToMesh - (axialDistance * directionToCamera);
                float offAxisLength = offAxis.magnitude;
                interloper.UpdateTargetAlpha(offAxisLength: offAxisLength);
                interloper.Update();
            }
            // var nonInterlopers = controllers.controllers.Values.Where(controller => controller != null && !interlopers.Contains(controller)).ToList();
            // Debug.Log($"nonInterlopers: {nonInterlopers.Count}");
            foreach (MaterialController controller in controllers.controllers.Values) {
                if (controller == null)
                    continue;
                if (controller.updatedThisLoop) {
                    controller.updatedThisLoop = false;
                    continue;
                }
                // j += 1;
                // if (j > 500) {
                //     j = 0;
                //     yield return waitForFrame;
                // }
                // Debug.Log("updating alpha");
                controller.UpdateTargetAlpha(2);
                controller.Update();
                controller.updatedThisLoop = false;
            }
            yield return null;
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
            if (controller != null && controller.childRenderers.Any(renderer => renderer != null && renderer.isPartOfStaticBatch)) {
                staticGeometry[controller] = collider.transform.position;
            }
        }
        Debug.Log($"[ClearSighter] initialized material controller cache with {controllers.controllers.Count}, static: {staticGeometry.Count}");
    }

    void HandleStaticGeometry() {
        Vector3 myPosition = myTransform.position;
        // Debug.Log($"static geometry: {inRooftopZone} {staticGeometry.Count}");
        cullingPlane.Set3Points(myTransform.position, myTransform.right, myTransform.up);
        foreach ((MaterialController controller, Vector3 position) in staticGeometry) {
            if (controller == null || controller.gameObject == null || controller.collider == null || !controller.gameObject.activeInHierarchy)
                continue;
            if (inRooftopZone) {
                controller.disableBecauseAbove = false;
            } else {
                controller.CeilingCheck(myPosition, cullingPlane, floorHeight);
            }
            controller.Update();
        }
    }

    void OnTriggerEnter(Collider other) => AddInterloper(other);

    void OnTriggerExit(Collider other) => RemoveInterloper(other);

    void AddInterloper(Collider other) {
        if (followTransform == null)
            return;
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
            .Where(f => f != null && f.gameObject != null)
            .ToList();
    }

    void OnDestroy() {
        StopCoroutine(coroutine);
    }
}
