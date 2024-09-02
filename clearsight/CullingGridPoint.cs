using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class CullingGridPoint {
    public Vector3 position;
    public bool isEmpty;
    public int floor;
    // todo: floor
    public List<string> NEInterlopers;
    public List<string> NWInterlopers;
    public List<string> SEInterlopers;
    public List<string> SWInterlopers;

    public List<string> NERoofZones;
    public List<string> NWRoofZones;
    public List<string> SERoofZones;
    public List<string> SWRoofZones;

    public CullingGridPoint() { } // needed for serialization
    public CullingGridPoint(Vector3 position, int floor) {
        this.position = position;
        this.floor = floor;

        LayerMask mask = LayerUtil.GetLayerMask(Layer.def, Layer.bulletPassThrough, Layer.clearsighterBlock);

        (NEInterlopers, NERoofZones) = DoRaycast(IsometricOrientation.NE, mask);

        (NWInterlopers, NWRoofZones) = DoRaycast(IsometricOrientation.NW, mask);

        (SEInterlopers, SERoofZones) = DoRaycast(IsometricOrientation.SE, mask);

        (SWInterlopers, SWRoofZones) = DoRaycast(IsometricOrientation.SW, mask);

        isEmpty = NEInterlopers.Count + NERoofZones.Count + NWInterlopers.Count + NWRoofZones.Count +
                  SEInterlopers.Count + SERoofZones.Count + SWInterlopers.Count + SWRoofZones.Count == 0;
    }
    Ray OrientationToRay(IsometricOrientation orientation) {
        float initialPlanarAngle = orientation switch {
            IsometricOrientation.NE => 45f,
            IsometricOrientation.SE => 135f,
            IsometricOrientation.SW => 225f,
            IsometricOrientation.NW => 315f,
            _ => 45f
        };
        float initialRotationOffset = 20f;
        float verticalRotationOffset = 30f;
        Vector3 PlanarDirection = Quaternion.Euler(0, initialPlanarAngle, 0) * Vector3.right;
        Quaternion rotationOffset = Quaternion.Euler(Vector3.up * initialRotationOffset);
        PlanarDirection = Vector3.Cross(Vector3.up, Vector3.Cross(PlanarDirection, Vector3.up));
        Quaternion planarRot = Quaternion.LookRotation(PlanarDirection, Vector3.up);
        Quaternion verticalRot = Quaternion.Euler(verticalRotationOffset, 0, 0);
        Quaternion rotation = rotationOffset * planarRot * verticalRot;
        return new Ray(position, -1f * (rotation * Vector3.forward));
    }
    (List<string>, List<string>) DoRaycast(IsometricOrientation orientation, LayerMask layerMask) {
        Ray ray = OrientationToRay(orientation);
        RaycastHit[] hits = Physics.RaycastAll(ray, 100f, layerMask);
        HashSet<CullingComponent> cullingComponents = new HashSet<CullingComponent>();
        HashSet<RooftopZone> roofZones = new HashSet<RooftopZone>();


        foreach (RaycastHit hit in hits) {
            if (hit.collider.bounds.Contains(position)) continue;
            CullingComponent cullingComponent = null;
            RooftopZone zone = null;
            Transform rootWithoutHierarchyFolder = hit.collider.transform.GetRoot(skipHierarchyFolders: true);
            if (rootWithoutHierarchyFolder != null) {
                cullingComponent = rootWithoutHierarchyFolder.GetComponent<CullingComponent>();
                zone = rootWithoutHierarchyFolder.GetComponent<RooftopZone>();
            } else {
                if (hit.collider.transform.root.gameObject.IsHierarchyFolder()) {
                    cullingComponent = hit.collider.GetComponent<CullingComponent>();
                    zone = hit.collider.GetComponent<RooftopZone>();
                } else {
                    cullingComponent = hit.collider.transform.root.GetComponent<CullingComponent>();
                    zone = hit.collider.transform.root.GetComponent<RooftopZone>();
                }
            }
            if (cullingComponent != null) {

                if (cullingComponent.debug) {
                    Color color = orientation switch {
                        IsometricOrientation.NE => Color.red,
                        IsometricOrientation.SE => Color.clear,
                        IsometricOrientation.SW => Color.green,
                        IsometricOrientation.NW => Color.clear
                    };
                    if (orientation == IsometricOrientation.SW)
                        Debug.DrawRay(position, 10f * ray.direction, color, 5);
                }

                TagSystem tagSystem = cullingComponent.GetComponent<TagSystem>();
                if (tagSystem != null && tagSystem.data.invisibleOnPlayerFloor) {

                } else {
                    cullingComponents.Add(cullingComponent);
                }
            }
            if (zone != null) { roofZones.Add(zone); }
        }
        List<string> cullingIds = cullingComponents.Select(component => component.idn).ToHashSet().ToList();
        List<string> roofIds = roofZones.Select(zone => zone.idn).ToHashSet().ToList();
        return (cullingIds, roofIds);
    }

    public void DrawRay(IsometricOrientation orientation) {
        Ray ray = OrientationToRay(orientation);
        Debug.DrawRay(position, 7.5f * ray.direction, Color.white);
    }
    public List<string> GetInterlopers(IsometricOrientation orientation) => orientation switch {
        IsometricOrientation.NE => NEInterlopers,
        IsometricOrientation.NW => NWInterlopers,
        IsometricOrientation.SE => SEInterlopers,
        IsometricOrientation.SW => SWInterlopers,
        _ => NEInterlopers
    };
    public List<string> GetRooftopZones(IsometricOrientation orientation) => orientation switch {
        IsometricOrientation.NE => NERoofZones,
        IsometricOrientation.NW => NWRoofZones,
        IsometricOrientation.SE => SERoofZones,
        IsometricOrientation.SW => SWRoofZones,
        _ => NERoofZones
    };
    public (Vector3, Vector3) rayCastOriginAndDirection(Vector3 playerPosition) {
        Vector3 pointPosition = position + (2f * Vector3.up);
        Vector3 displacement = pointPosition - playerPosition;
        Vector3 origin = playerPosition;
        origin.y = pointPosition.y;
        displacement.y = 0;
        return (origin, displacement);
    }
}