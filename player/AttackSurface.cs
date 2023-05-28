using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using cakeslice;
using Obi;
using UnityEngine;
public class AttackSurface : MonoBehaviour {
    public string niceName;
    public Camera attackCam;
    [HideInInspector]
    public RenderTexture renderTexture;
    public Transform mainCameraPosition;
    public Outline outline;
    public Transform attackElementRoot;
    public float interactionDistance = 2f;
    // public List<ObiRope> obiRopes;
    // Dictionary<ObiRope, MeshCollider>
    Dictionary<MeshCollider, MeshFilter> ropeMeshes;
    Dictionary<MeshCollider, MeshRenderer> ropeRenderers;
    public void Start() {
        renderTexture = new RenderTexture(1250, 750, 16, RenderTextureFormat.Default);
        attackCam.targetTexture = renderTexture;
        attackCam.enabled = false;
        ropeMeshes = new Dictionary<MeshCollider, MeshFilter>();
        ropeRenderers = new Dictionary<MeshCollider, MeshRenderer>();
        foreach (ObiRope rope in GetComponentsInChildren<ObiRope>()) {
            MeshCollider collider = rope.GetComponent<MeshCollider>();
            MeshFilter filter = rope.GetComponent<MeshFilter>();
            MeshRenderer renderer = rope.GetComponent<MeshRenderer>();
            ropeMeshes[collider] = filter;
            ropeRenderers[collider] = renderer;
        }
        DisableOutline();
    }
    public void EnableAttackSurface() {
        attackCam.enabled = true;
    }
    public void DisableAttackSurface() {
        attackCam.enabled = false;
    }

    public void EnableOutline() {
        if (outline != null) {
            outline.enabled = true;
        }
    }
    public void DisableOutline() {
        if (outline != null) {
            outline.enabled = false;
        }
    }

    public void RefreshRopeColliders() {
        foreach (KeyValuePair<MeshCollider, MeshFilter> kvp in ropeMeshes) {
            kvp.Key.sharedMesh = null;
            kvp.Key.sharedMesh = kvp.Value.sharedMesh;
        }
    }

    public BurglarAttackResult HandleRopeCutting(Ray projection) {
        RefreshRopeColliders();
        BurglarAttackResult result = BurglarAttackResult.None;
        RaycastHit[] hits = Physics.RaycastAll(projection, 1000, LayerUtil.GetLayerMask(Layer.attackSurface, Layer.bulletOnly));
        foreach (RaycastHit hit in hits.OrderBy(hit => hit.distance)) {
            ObiRope rope = hit.collider.gameObject.GetComponent<ObiRope>();
            if (rope != null) {
                MeshRenderer meshRenderer = rope.GetComponent<MeshRenderer>();
                if (!meshRenderer.enabled) continue;
                float closestDistance = float.MaxValue;
                ObiStructuralElement targetElement = null;
                int index = 0;
                Vector3 targetCentroid = Vector3.zero;
                Vector3 hitPoint = rope.solver.transform.InverseTransformPoint(hit.point);
                for (int i = 0; i < rope.elements.Count; ++i) {
                    Vector3 centroid = (Vector3)(rope.solver.positions[rope.elements[i].particle1] + rope.solver.positions[rope.elements[i].particle2]) / 2f;
                    float distance = Vector3.Distance(centroid, hitPoint);
                    if (distance < closestDistance) {
                        closestDistance = distance;
                        targetElement = rope.elements[i];
                        index = i;
                        targetCentroid = centroid;
                    }
                }

                rope.Tear(rope.elements[index]);
                rope.RebuildConstraintsFromElements();
                AttackSurfaceWire wireElement = rope.GetComponent<AttackSurfaceWire>();
                result = wireElement?.DoCut();
            }
            // Debug.Log($"break on: {hit.collider.gameObject}");
            break;
        }
        return result;
    }
}