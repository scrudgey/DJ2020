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

    Coroutine coroutine;
    bool inRooftopZone;
    Collider[] colliderHits;
    void Start() {
        colliderHits = new Collider[5000];
        myTransform = transform;
        InitializeMaterialControllerCache();
        InvokeRepeating("HandleStaticGeometry", 0f, 1f);
        rooftopZones = GameObject.FindObjectsOfType<RooftopZone>().Select(zone => zone.GetComponent<Collider>()).ToList();
        coroutine = StartCoroutine(RunJobRepeatedly());
    }
    IEnumerator RunJobRepeatedly() {
        while (true) {
            if (followTransform == null) {
                yield return null;
                continue;
            }
            // int i = 0;
            int j = 0;

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
            int numberHits = Physics.OverlapSphereNonAlloc(myTransform.position, 20f, colliderHits, LayerUtil.GetMask(Layer.obj, Layer.bulletPassThrough, Layer.shell), QueryTriggerInteraction.Ignore);
            for (int k = 0; k < numberHits; k++) {
                Collider collider = colliderHits[k];
                if (collider == null || collider.gameObject == null || collider.transform.IsChildOf(myTransform) || collider.transform.IsChildOf(followTransform))
                    continue;
                j += 1;
                if (j > 500) {
                    j = 0;
                    yield return new WaitForEndOfFrame();
                }
                MaterialController controller = controllers.get(collider);
                if (controller != null) {
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
                                                                        interloper.gameObject != null)) {
                j += 1;
                if (j > 500) {
                    j = 0;
                    yield return new WaitForEndOfFrame();
                }
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
            var nonInterlopers = controllers.controllers.Values.Where(controller => controller != null && !interlopers.Contains(controller)).ToList();
            foreach (MaterialController controller in nonInterlopers) {
                j += 1;
                if (j > 500) {
                    j = 0;
                    yield return new WaitForEndOfFrame();
                }
                controller.UpdateTargetAlpha(0);
                controller.Update();
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
            if (collider.gameObject.isStatic) {
                staticGeometry[controller] = collider.transform.position;
            }
        }
        Debug.Log($"[ClearSighter] initialized material controller cache with {controllers.controllers.Count}");
    }

    void HandleStaticGeometry() {
        Vector3 myPosition = myTransform.position;
        foreach ((MaterialController controller, Vector3 position) in staticGeometry) {
            if (controller == null || controller.gameObject == null || controller.collider == null || !controller.gameObject.activeInHierarchy)
                continue;
            if (inRooftopZone) {
                controller.disableBecauseAbove = false;
            } else {
                controller.CeilingCheck(myPosition);
            }
            controller.Update();
        }
    }

    void OnTriggerEnter(Collider other) => AddInterloper(other);

    void OnTriggerExit(Collider other) => RemoveInterloper(other);

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
