using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
public class NeoClearsighter : MonoBehaviour {
    static readonly int BATCHSIZE = 500;
    enum State { normal, showAll, interloperOnly }
    State state;
    PointOctree<Renderer> rendererTree;
    BoundsOctree<Renderer> rendererBoundsTree;
    public Transform followTransform;
    public Shader interloperShader;
    Transform myTransform;
    WaitForEndOfFrame waitForFrame = new WaitForEndOfFrame();
    HashSet<Renderer> previousAboveRendererBatch;
    HashSet<Renderer> previousInterloperBatch;
    HashSet<Renderer> previousDynamicRendererBatch;
    HashSet<Transform> hiddenTransforms;
    Dictionary<Renderer, Vector3> rendererPositions;
    Dictionary<Renderer, Bounds> rendererBounds;
    Dictionary<Renderer, Transform> rendererTransforms;
    Dictionary<Renderer, ShadowCastingMode> initialShadowCastingMode;
    Dictionary<Renderer, TagSystemData> rendererTagData;
    Dictionary<Renderer, Material> initialMaterials;
    Dictionary<Renderer, Material> interloperMaterials;
    Dictionary<Collider, Renderer[]> dynamicColliderToRenderer;
    Dictionary<Collider, Transform> dynamicColliderRoot;
    Dictionary<Renderer, GameObject> cutawayRenderers;
    Collider[] colliderHits;
    CharacterCamera myCamera;
    CharacterController characterController;
    Transform cameraTransform;
    bool initialized;
    Vector3 previousOrigin;
    MaterialPropertyBlock propBlock;
    List<Collider> rooftopZones = new List<Collider>();

    public void Initialize(Transform followTransform, CharacterCamera camera, CharacterController characterController) {
        this.followTransform = followTransform;
        this.myCamera = camera;
        this.characterController = characterController;

        cameraTransform = myCamera.transform;
        myTransform = transform;
        colliderHits = new Collider[5000];

        InitializeTree();
        StartCoroutine(Toolbox.RunJobRepeatedly(HandleGeometry));

        rooftopZones = GameObject.FindObjectsOfType<RooftopZone>()
           .SelectMany(zone => zone.GetComponentsInChildren<Collider>())
           .ToList();
        initialized = true;
    }

    void InitializeTree() {
        rendererPositions = new Dictionary<Renderer, Vector3>();
        rendererBounds = new Dictionary<Renderer, Bounds>();
        dynamicColliderToRenderer = new Dictionary<Collider, Renderer[]>();
        dynamicColliderRoot = new Dictionary<Collider, Transform>();
        rendererTransforms = new Dictionary<Renderer, Transform>();
        initialShadowCastingMode = new Dictionary<Renderer, ShadowCastingMode>();
        rendererTagData = new Dictionary<Renderer, TagSystemData>();
        previousAboveRendererBatch = new HashSet<Renderer>();
        previousInterloperBatch = new HashSet<Renderer>();
        previousDynamicRendererBatch = new HashSet<Renderer>();
        hiddenTransforms = new HashSet<Transform>();
        initialMaterials = new Dictionary<Renderer, Material>();
        interloperMaterials = new Dictionary<Renderer, Material>();
        cutawayRenderers = new Dictionary<Renderer, GameObject>();
        propBlock = new MaterialPropertyBlock();

        List<Renderer> staticRenderers = GameObject.FindObjectsOfType<Renderer>()
            .Where(renderer => renderer.isPartOfStaticBatch)
            .Concat(
                GameObject.FindObjectsOfType<SpriteRenderer>()
                    .Where(obj => obj.CompareTag("decor"))
                    .Select(obj => obj.GetComponent<Renderer>())
            )
            .Concat(
                GameObject.FindObjectsOfType<TextMeshPro>()
                    .Select(obj => obj.GetComponent<Renderer>())
            )
            .ToList();

        // specialRenderers = GameObject.FindGameObjectsWithTag("occlusionSpecial").Select(obj => obj.GetComponent<Renderer>()).Where(renderer => renderer != null).ToList();
        // elevatorShowZones = GameObject.FindObjectsOfType<ElevatorShowZone>().ToArray();

        rendererTree = new PointOctree<Renderer>(100, Vector3.zero, 1);
        rendererBoundsTree = new BoundsOctree<Renderer>(100, Vector3.zero, 0.5f, 1);
        foreach (Renderer renderer in staticRenderers) {
            if (renderer.name.Contains("cutaway")) continue;
            Vector3 position = renderer.bounds.center;
            rendererTree.Add(renderer, position);
            rendererBoundsTree.Add(renderer, renderer.bounds);
            rendererPositions[renderer] = position;
            rendererBounds[renderer] = renderer.bounds;
            rendererTagData[renderer] = Toolbox.GetTagData(renderer.gameObject);
            initialMaterials[renderer] = renderer.sharedMaterial;
            interloperMaterials[renderer] = NewInterloperMaterial(renderer);
            initialShadowCastingMode[renderer] = renderer.shadowCastingMode;
            Transform findAnchor = renderer.gameObject.transform.root.Find("clearSighterAnchor");
            if (findAnchor != null) {
                Debug.Log($"found clear sighter anchor: {renderer.gameObject} {findAnchor}");
                rendererPositions[renderer] = findAnchor.position;
            }
        }
        foreach (Renderer renderer in rendererTree.GetAll()) {
            if (renderer.CompareTag("cutaway")) {
                Transform cutaway = renderer.transform.parent.Find("cutaway");
                cutaway.gameObject.SetActive(false);
                cutawayRenderers[renderer] = cutaway.gameObject;
            }
        }
    }

    public static Material NewInterloperMaterial(Renderer renderer) {
        Material interloperMaterial = new Material(renderer.sharedMaterial);
        Texture albedo = renderer.sharedMaterial.mainTexture;
        interloperMaterial.shader = Resources.Load("Scripts/shaders/InterloperShadow") as Shader;
        interloperMaterial.SetTexture("_Texture", albedo);
        return interloperMaterial;
    }

    IEnumerator HandleGeometry() {
        if (initialized) {
            if ((myCamera.state == CameraState.normal || myCamera.state == CameraState.attractor)) {
                state = State.normal;
            } else {
                state = State.showAll;
            }

            // bool inRooftopZone = false;
            foreach (Collider zone in rooftopZones) {
                if (zone == null) continue;
                if (zone.bounds.Contains(followTransform.position)) {
                    state = State.interloperOnly;
                    break;
                }
            }

            switch (state) {
                case State.normal:
                    yield return HandleGeometryNormal();
                    break;
                case State.showAll:
                    yield return ShowAllGeometry();
                    break;
                case State.interloperOnly:
                    yield return InterloperOnly();
                    break;
            }
        } else {
            yield return null;
        }

    }

    IEnumerator ShowAllGeometry() {
        // reset previous batch
        yield return ResetPreviousBatch(0);
        previousAboveRendererBatch = new HashSet<Renderer>();
        previousInterloperBatch = new HashSet<Renderer>();
        previousDynamicRendererBatch = new HashSet<Renderer>();
        hiddenTransforms.Clear();
        yield return waitForFrame;
    }

    IEnumerator HandleGeometryNormal() {
        HashSet<Renderer> nextAboveRenderBatch = new HashSet<Renderer>();
        HashSet<Renderer> nextInterloperBatch = new HashSet<Renderer>();
        HashSet<Renderer> nextDynamicRenderBatch = new HashSet<Renderer>();

        // if (previousOrigin == null || previousOrigin == Vector3.zero) {
        //     previousOrigin = origin;
        // }
        // if (Mathf.Abs(origin.y - previousOrigin.y) > 0.5f) {
        //     previousOrigin = origin;
        // } else {
        //     origin.y = previousOrigin.y;
        // }

        Vector3 origin = followTransform.position;
        origin.y = Mathf.Round(origin.y * 10f) / 10f;

        float lift = characterController.state == CharacterState.hvac ? 0f : 1.5f;
        Vector3 liftedOrigin = origin + new Vector3(0f, lift, 0f);
        int j = 0;

        // static geometry above me
        yield return HandleStaticAbove(j, liftedOrigin, nextAboveRenderBatch);

        // static geometry interlopers
        yield return HandleInterlopers(j, nextInterloperBatch, nextAboveRenderBatch);

        // yield return HandleSpecialRenderers(liftedOrigin);

        // dynamic renderers above me
        yield return HandleDynamicAbove(j, liftedOrigin, nextAboveRenderBatch.Union(nextInterloperBatch).ToHashSet(), nextDynamicRenderBatch);

        // reset previous batch
        yield return ResetPreviousBatch(j);

        previousAboveRendererBatch = nextAboveRenderBatch;
        previousDynamicRendererBatch = nextDynamicRenderBatch;
        previousInterloperBatch = nextInterloperBatch;
        yield return waitForFrame;
    }

    IEnumerator HandleStaticAbove(int j, Vector3 liftedOrigin, HashSet<Renderer> nextAboveRenderBatch) {
        Ray upRay = new Ray(liftedOrigin, Vector3.up);
        Renderer[] above = rendererTree.GetNearby(upRay, 50f);
        for (int i = 0; i < above.Length; i++) {
            j++;
            if (j > BATCHSIZE) {
                j = 0;
                yield return waitForFrame;
            }
            Renderer renderer = above[i];
            if (renderer == null) continue;
            // floor of the collider
            Vector3 position = rendererPositions[renderer] - new Vector3(0f, rendererBounds[renderer].extents.y, 0f);

            // we disable geometry above if the floor of the renderer bounds is above the lifted origin point
            // which is player position + 1.5
            if (position.y > liftedOrigin.y) {
                MakeTransparent(renderer, Vector3.zero, true);

                nextAboveRenderBatch.Add(renderer);
                hiddenTransforms.Add(renderer.transform.root);
                if (previousAboveRendererBatch.Contains(renderer)) {
                    previousAboveRendererBatch.Remove(renderer);
                }
            }
        }
    }

    IEnumerator HandleDynamicAbove(int j, Vector3 liftedOrigin, HashSet<Renderer> alreadyHandledRenderer, HashSet<Renderer> nextDynamicRenderBatch) {
        int numberHits = Physics.OverlapSphereNonAlloc(liftedOrigin, 40f, colliderHits, LayerUtil.GetLayerMask(Layer.obj, Layer.bulletPassThrough, Layer.shell, Layer.bulletOnly, Layer.interactive), QueryTriggerInteraction.Ignore);
        for (int k = 0; k < numberHits; k++) {
            Collider collider = colliderHits[k];
            if (collider == null || collider.name == "cutaway" || collider.gameObject == null || collider.transform.IsChildOf(myTransform) || collider.transform.IsChildOf(followTransform))
                continue;
            j += 1;
            if (j > BATCHSIZE) {
                j = 0;
                yield return waitForFrame;
            }
            Renderer[] renderers = GetDynamicRenderers(collider);
            Transform root = dynamicColliderRoot[collider];

            if (root.position.y > liftedOrigin.y) {
                foreach (Renderer renderer in renderers) {
                    if (renderer == null) continue;
                    if (alreadyHandledRenderer.Contains(renderer)) continue;
                    if (renderer.CompareTag("occlusionSpecial")) continue;

                    MakeTransparent(renderer, Vector3.zero, true);

                    nextDynamicRenderBatch.Add(renderer);
                    hiddenTransforms.Add(root);
                    if (previousDynamicRendererBatch.Contains(renderer)) {
                        previousDynamicRendererBatch.Remove(renderer);
                    }
                }
            }
        }
    }

    IEnumerator InterloperOnly() {
        HashSet<Renderer> nextInterloperBatch = new HashSet<Renderer>();

        int j = 0;
        // handle interlopers
        yield return HandleInterlopers(j, nextInterloperBatch, null);

        // reset previous batch
        yield return ResetPreviousBatch(j);

        previousInterloperBatch = nextInterloperBatch;
        yield return waitForFrame;
    }

    IEnumerator ResetPreviousBatch(int j) {
        foreach (Renderer renderer in previousAboveRendererBatch.Concat(previousDynamicRendererBatch).Concat(previousInterloperBatch)) {
            j++;
            if (j > BATCHSIZE) {
                j = 0;
                yield return waitForFrame;
            }
            if (renderer == null) continue;
            if (renderer.CompareTag("cutaway")) {
                cutawayRenderers[renderer].SetActive(false);
            }
            if (renderer.CompareTag("occlusionSpecial")) continue;
            // if (renderer.name == "elevator_car") {
            //     Debug.Log("weird");
            //     Debug.Break();
            // }
            renderer.shadowCastingMode = initialShadowCastingMode[renderer];
            renderer.material = initialMaterials[renderer];
            hiddenTransforms.Remove(renderer.transform.root);
        }
    }

    void MakeTransparent(Renderer renderer, Vector3 directionToInterloper, bool above) {
        // handle non-static
        // handle elevator car
        if (above) {
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
        } else if (renderer.CompareTag("cutaway")) {
            renderer.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
            cutawayRenderers[renderer].SetActive(true);
        } else {
            if (directionToInterloper.sqrMagnitude < 100f && directionToInterloper.y < 1.5) {
                renderer.material = interloperMaterials[renderer];
                // float targetAlpha = Math.Min(0.5f, 1 / (directionToInterloper.sqrMagnitude / 10fs));
                float targetAlpha = 0.5f;
                MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
                renderer.GetPropertyBlock(propBlock);
                propBlock.SetFloat("_TargetAlpha", targetAlpha);
                renderer.SetPropertyBlock(propBlock);
                renderer.shadowCastingMode = initialShadowCastingMode[renderer];
            } else {
                // if (renderer.name == "elevator_car") {
                //     Debug.Log("weird");
                //     Debug.Break();
                // }

                renderer.material = initialMaterials[renderer];
                renderer.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
            }
        }
    }

    Plane[] interloperFrustrum() {
        // float size = 2f;
        float size = 4f;

        // Ordering: [0] = Left, [1] = Right, [2] = Down, [3] = Up, [4] = Near, [5] = Far
        float distance = (cameraTransform.position - followTransform.position).magnitude;
        float angle = (float)Math.Atan((myCamera.Camera.orthographicSize) / distance) * (180f / (float)Math.PI) * 2;

        Vector3 leftNormal = Quaternion.AngleAxis(1f * angle, cameraTransform.up) * cameraTransform.right;
        Vector3 rightNormal = Quaternion.AngleAxis(-1f * angle, cameraTransform.up) * (-1f * cameraTransform.right);

        Plane left = new Plane(leftNormal, followTransform.position - size * cameraTransform.right);
        Plane right = new Plane(rightNormal, followTransform.position + size * cameraTransform.right);

        Plane down = new Plane(cameraTransform.up, followTransform.position - size * cameraTransform.up);
        Plane up = new Plane(-1f * cameraTransform.up, followTransform.position + size * cameraTransform.up);
        Plane near = new Plane(cameraTransform.forward, cameraTransform.position);
        Plane far = new Plane(-1f * cameraTransform.forward, followTransform.position);

        // Toolbox.DrawPlane(followTransform.position - size * cameraTransform.right, left);
        // Toolbox.DrawPlane(followTransform.position + size * cameraTransform.right, right);
        return new Plane[] { left, right, down, up, near, far };
    }

    // IEnumerator HandleSpecialRenderers(Vector3 liftedOrigin) {
    //     bool showRenderers = elevatorShowZones.Any(zone => zone.collider.bounds.Contains(liftedOrigin));
    //     bool transparent = elevatorShowZones.Where(zone => zone.transparent).Any(zone => zone.collider.bounds.Contains(liftedOrigin));
    //     foreach (Renderer renderer in specialRenderers) {
    //         if (showRenderers) {
    //             if (transparent) {
    //                 renderer.material = interloperMaterials[renderer];
    //                 float targetAlpha = 0.7f;
    //                 MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
    //                 renderer.GetPropertyBlock(propBlock);
    //                 propBlock.SetFloat("_TargetAlpha", targetAlpha);
    //                 renderer.SetPropertyBlock(propBlock);
    //                 renderer.shadowCastingMode = initialShadowCastingMode[renderer];
    //             } else {
    //                 renderer.material = initialMaterials[renderer];
    //                 renderer.shadowCastingMode = initialShadowCastingMode[renderer];
    //             }
    //         } else {
    //             renderer.material = initialMaterials[renderer];
    //             renderer.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
    //         }
    //     }
    //     yield return null;
    // }
    IEnumerator HandleInterlopers(int j, HashSet<Renderer> nextInterloperBatch, HashSet<Renderer> nextAboveRenderBatch) {
        List<Renderer> interlopers = rendererBoundsTree.GetWithinFrustum(interloperFrustrum());

        Plane XPlane = new Plane(Vector3.right, followTransform.position);
        Plane ZPlane = new Plane(Vector3.forward, followTransform.position);

        bool cameraXSide = XPlane.GetSide(cameraTransform.position);
        bool cameraZSide = ZPlane.GetSide(cameraTransform.position);

        Vector3 planarDisplacement = cameraTransform.position - followTransform.position;
        planarDisplacement.y = 0;
        planarDisplacement = planarDisplacement.normalized;

        Plane detectionPlane;
        if (Math.Abs(planarDisplacement.x) > Math.Abs(planarDisplacement.z)) {
            detectionPlane = XPlane;
        } else {
            detectionPlane = ZPlane;
        }
        bool detectionSide = detectionPlane.GetSide(cameraTransform.position);


        // Debug.Log(planarDisplacement);
        // Debug.DrawRay(followTransform.position, CharacterCamera.rotationOffset * planarDisplacement, Color.white, 0.75f);

        // if (cameraXSide) {
        //     XPlane = new Plane(Vector3.right, followTransform.position - margin * Vector3.right);
        // } else {
        //     XPlane = new Plane(Vector3.right, followTransform.position + margin * Vector3.right);
        // }

        // if (cameraZSide) {
        //     ZPlane = new Plane(Vector3.forward, followTransform.position - margin * Vector3.forward);
        // } else {
        //     ZPlane = new Plane(Vector3.forward, followTransform.position + margin * Vector3.forward);
        // }

        for (int i = 0; i < interlopers.Count; i++) {
            j++;
            if (j > BATCHSIZE) {
                j = 0;
                yield return waitForFrame;
            }

            Renderer renderer = interlopers[i];
            if (renderer == null) continue;
            if (renderer.name == "cutaway") continue;

            TagSystemData tagSystemData = rendererTagData[renderer];
            // if (tagSystemData.dontHideInterloper) continue;
            Vector3 rendererPosition = rendererBounds[renderer].center;
            Vector3 directionToInterloper = rendererPosition - followTransform.position;

            float xParity = cameraXSide ? 1f : -1f;
            float zParity = cameraZSide ? 1f : -1f;
            bool rendererXSide = XPlane.GetSide(rendererPosition + new Vector3(xParity * rendererBounds[renderer].extents.x, 0f, 0f));
            bool rendererZSide = ZPlane.GetSide(rendererPosition + new Vector3(0f, 0f, zParity * rendererBounds[renderer].extents.z));
            // bool rendererZSide = true;

            // bool detectionSide = detectionPlane.GetSide(rendererBounds[renderer].center);

            // if (rendererXSide == cameraXSide && rendererZSide == cameraZSide && directionToInterloper.y > 0.2f) {
            if (detectionPlane.GetSide(rendererBounds[renderer].center) == detectionSide && directionToInterloper.y > 0.2f) {
                if (renderer == null) continue;
                if (nextAboveRenderBatch?.Contains(renderer) ?? false) continue;
                MakeTransparent(renderer, directionToInterloper, false);
                nextInterloperBatch.Add(renderer);
                hiddenTransforms.Add(renderer.transform.root);
                if (previousInterloperBatch.Contains(renderer)) {
                    previousInterloperBatch.Remove(renderer);
                }
            }
        }
    }

    public Renderer[] GetDynamicRenderers(Collider key) {
        if (dynamicColliderToRenderer.ContainsKey(key)) {
            return dynamicColliderToRenderer[key];
        } else {
            Renderer[] renderers = key.transform.root.GetComponentsInChildren<Renderer>().Where(x => x != null &&
                                                !(x is ParticleSystemRenderer) &&
                                                !(x is LineRenderer)
                                                ).ToArray();
            dynamicColliderToRenderer[key] = renderers;
            dynamicColliderRoot[key] = key.transform.root;

            Transform findAnchor = key.transform.root.Find("clearSighterAnchor");
            if (findAnchor != null) {
                dynamicColliderRoot[key] = findAnchor;
            }

            foreach (Renderer renderer in renderers) {
                rendererTransforms[renderer] = renderer.transform;
                if (findAnchor != null) {
                    rendererTransforms[renderer] = findAnchor;
                }
                if (!initialMaterials.ContainsKey(renderer)) {
                    initialMaterials[renderer] = renderer.sharedMaterial;
                }
                if (!interloperMaterials.ContainsKey(renderer)) {
                    interloperMaterials[renderer] = NewInterloperMaterial(renderer);
                }
                if (!initialShadowCastingMode.ContainsKey(renderer))
                    initialShadowCastingMode[renderer] = renderer.shadowCastingMode;
            }
            return renderers;
        }
    }

    public void HandleInputModeChange(InputMode oldInputMode, InputMode newInputMode) {

    }

    public bool IsObjectVisible(GameObject obj) {
        return !hiddenTransforms.Contains(obj.transform.root);
    }
}
