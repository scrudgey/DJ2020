using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class CullingGridPoint {
    public Vector3 position;
    public List<string> NEInterlopers;
    public List<string> NWInterlopers;
    public List<string> SEInterlopers;
    public List<string> SWInterlopers;

    public List<string> NERoofZones;
    public List<string> NWRoofZones;
    public List<string> SERoofZones;
    public List<string> SWRoofZones;

    public CullingGridPoint() { } // needed for serialization
    public CullingGridPoint(Vector3 position) {
        this.position = position;

        LayerMask mask = LayerUtil.GetLayerMask(Layer.def, Layer.bulletPassThrough, Layer.clearsighterBlock);

        (NEInterlopers, NERoofZones) = DoRaycast(CharacterCamera.IsometricOrientation.NE, mask);

        (NWInterlopers, NWRoofZones) = DoRaycast(CharacterCamera.IsometricOrientation.NW, mask);

        (SEInterlopers, SERoofZones) = DoRaycast(CharacterCamera.IsometricOrientation.SE, mask);

        (SWInterlopers, SWRoofZones) = DoRaycast(CharacterCamera.IsometricOrientation.SW, mask);
    }
    Ray OrientationToRay(CharacterCamera.IsometricOrientation orientation) {
        float initialPlanarAngle = orientation switch {
            CharacterCamera.IsometricOrientation.NE => 45f,
            CharacterCamera.IsometricOrientation.SE => 135f,
            CharacterCamera.IsometricOrientation.SW => 225f,
            CharacterCamera.IsometricOrientation.NW => 315f,
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
    (List<string>, List<string>) DoRaycast(CharacterCamera.IsometricOrientation orientation, LayerMask layerMask) {
        Ray ray = OrientationToRay(orientation);
        RaycastHit[] hits = Physics.RaycastAll(ray, 100f, layerMask);
        HashSet<CullingComponent> cullingComponents = new HashSet<CullingComponent>();
        HashSet<RooftopZone> roofZones = new HashSet<RooftopZone>();
        // Color color = orientation switch {
        //     CharacterCamera.IsometricOrientation.NE => Color.red,
        //     CharacterCamera.IsometricOrientation.SE => Color.clear,
        //     CharacterCamera.IsometricOrientation.SW => Color.green,
        //     CharacterCamera.IsometricOrientation.NW => Color.clear
        // };
        // Debug.DrawRay(position, 10f * ray.direction, Color.white);

        foreach (RaycastHit hit in hits) {
            Transform root = hit.collider.transform.GetRoot(skipHierarchyFolders: true);

            CullingComponent component = root.GetComponent<CullingComponent>();
            if (component != null) { cullingComponents.Add(component); }

            RooftopZone zone = root.GetComponent<RooftopZone>();
            if (zone != null) { roofZones.Add(zone); }
        }
        List<string> cullingIds = cullingComponents.Select(component => component.idn).ToHashSet().ToList();
        List<string> roofIds = roofZones.Select(zone => zone.idn).ToHashSet().ToList();
        return (cullingIds, roofIds);
    }

    public void DrawRay(CharacterCamera.IsometricOrientation orientation) {
        Ray ray = OrientationToRay(orientation);
        Debug.DrawRay(position, 7.5f * ray.direction, Color.white);
    }
    public List<string> GetInterlopers(CharacterCamera.IsometricOrientation orientation) => orientation switch {
        CharacterCamera.IsometricOrientation.NE => NEInterlopers,
        CharacterCamera.IsometricOrientation.NW => NWInterlopers,
        CharacterCamera.IsometricOrientation.SE => SEInterlopers,
        CharacterCamera.IsometricOrientation.SW => SWInterlopers,
        _ => NEInterlopers
    };
    public List<string> GetRooftopZones(CharacterCamera.IsometricOrientation orientation) => orientation switch {
        CharacterCamera.IsometricOrientation.NE => NERoofZones,
        CharacterCamera.IsometricOrientation.NW => NWRoofZones,
        CharacterCamera.IsometricOrientation.SE => SERoofZones,
        CharacterCamera.IsometricOrientation.SW => SWRoofZones,
        _ => NERoofZones
    };
    public Vector3 rayCastDirection(Vector3 playerPosition) {
        Vector3 pointPosition = position + (2f * Vector3.up);
        Vector3 displacement = pointPosition - playerPosition;
        displacement.y = 0;
        return displacement;
    }
}