using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
public class ElevatorOccluder : MonoBehaviour {
    CharacterCamera myCamera;

    public ElevatorController elevatorController;
    public List<Renderer> renderers;
    ElevatorShowZone[] showZones;
    Dictionary<Renderer, Material> initialMaterials;
    Dictionary<Renderer, ShadowCastingMode> initialShadowCastingMode;
    Dictionary<Renderer, Material> interloperMaterials;
    public ElevatorShowZone elevatorCarZone;
    public Renderer counterWeightRenderer;

    // if elevator is not on not on player's floor, invisible
    // if elevator is on player's floor and elevator doors shut, invisible
    // if elevator doors open, visible
    // if occluding player, transparent
    void Start() {
        initialMaterials = new Dictionary<Renderer, Material>();
        initialShadowCastingMode = new Dictionary<Renderer, ShadowCastingMode>();
        interloperMaterials = new Dictionary<Renderer, Material>();
        showZones = GameObject.FindObjectsOfType<ElevatorShowZone>().ToArray();
        foreach (Renderer renderer in renderers) {
            initialMaterials[renderer] = renderer.sharedMaterial;
            initialShadowCastingMode[renderer] = renderer.shadowCastingMode;
            interloperMaterials[renderer] = NeoClearsighter.NewInterloperMaterial(renderer);
        }
    }
    void Update() {
        // float verticalDisplacement = transform.position.y - GameManager.I.playerPosition.y;
        ElevatorFloorData currentFloor = elevatorController.ClosestFloor(GameManager.I.playerPosition);
        ElevatorDoors currentDoors = currentFloor.doors;

        // TODO: handle other view modes

        if (elevatorCarZone.collider.bounds.Contains(GameManager.I.playerPosition)) {
            MakeTranslucent();
        } else if (showZones.Any(zone => zone.collider.bounds.Contains(GameManager.I.playerPosition))) {
            MakeOpaque();
        } else if (!currentDoors.doorsAreClosed) {
            MakeOpaque();
        } else {
            MakeInvisible();
        }

        //  else if (Mathf.Abs(verticalDisplacement) < 1f) {
        //     // on player floor
        //     if (currentDoors.doorsAreClosed) {
        //         MakeInvisible();
        //     } else {
        //         MakeOpaque();
        //     }
        // } else {
        //     MakeInvisible();
        // }

    }

    void MakeInvisible() {
        foreach (Renderer renderer in renderers) {
            renderer.material = initialMaterials[renderer];
            renderer.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
        }
    }

    void MakeOpaque() {
        foreach (Renderer renderer in renderers) {
            renderer.material = initialMaterials[renderer];
            renderer.shadowCastingMode = initialShadowCastingMode[renderer];
        }
    }

    void MakeTranslucent() {
        foreach (Renderer renderer in renderers) {
            if (renderer == counterWeightRenderer) {
                renderer.material = initialMaterials[renderer];
                renderer.shadowCastingMode = initialShadowCastingMode[renderer];
                continue;
            }
            renderer.material = interloperMaterials[renderer];
            float targetAlpha = 0.7f;
            MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(propBlock);
            propBlock.SetFloat("_TargetAlpha", targetAlpha);
            renderer.SetPropertyBlock(propBlock);
            renderer.shadowCastingMode = initialShadowCastingMode[renderer];
        }
    }
}
