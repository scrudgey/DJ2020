using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InteractorTargetData {
    public Collider collider;
    public bool targetIsInRange;
    public Interactive target;
    public InteractorTargetData(Interactive target, Collider collider, Vector3 playerPosition) {
        this.target = target;
        this.collider = collider;
        Vector3 position = collider.ClosestPoint(playerPosition);
        this.targetIsInRange = Vector3.Distance(position, playerPosition) < 2f;
        this.target = target;
    }
    static public bool Equality(InteractorTargetData a, InteractorTargetData b) {
        if (a == null && b == null) {
            return true;
        } else if (a == null || b == null) {
            return false;
        } else {
            return a.target == b.target && a.collider == b.collider;
        }
    }
}
public class Interactor : MonoBehaviour, IBindable<Interactor> {
    public Action<Interactor> OnValueChanged { get; set; }
    public Action<InteractorTargetData> OnActionDone;
    public InteractorTargetData cursorTarget;
    public AttackSurface selectedAttackSurface;

    // public Dictionary<Collider, Interactive> interactives = new Dictionary<Collider, Interactive>();
    public CharacterController characterController;
    // public void AddInteractive(Collider other) {
    //     Interactive interactive = other.GetComponent<Interactive>();
    //     if (interactive) {
    //         interactives[other] = interactive;
    //         interactive.interactor = this;
    //     }
    //     RemoveNullInteractives();
    //     OnValueChanged?.Invoke(this);
    // }
    // public void RemoveInteractive(Collider other) {
    //     if (interactives.ContainsKey(other)) {
    //         interactives[other].interactor = null;
    //         interactives.Remove(other);
    //     }
    //     RemoveNullInteractives();
    //     OnValueChanged?.Invoke(this);
    // }
    // public void RemoveInteractive(Interactive other) {
    //     foreach (var item in interactives.Where(kvp => kvp.Value == other).ToList()) {
    //         item.Value.interactor = null;
    //         interactives.Remove(item.Key);
    //     }
    //     OnValueChanged?.Invoke(this);
    // }

    // public InteractorTargetData ActiveTarget() {
    //     RemoveNullInteractives();
    //     // if (interactives.Count == 0) {
    //     //     return null;
    //     // }
    //     List<InteractorTargetData> data = new List<InteractorTargetData>();
    //     foreach (KeyValuePair<Collider, Interactive> kvp in interactives) {
    //         if (!kvp.Value.interactible) continue;
    //         if (kvp.Key != null && (kvp.Key.bounds.center.y - transform.position.y) > -1) {
    //             data.Add(new InteractorTargetData(kvp.Value, kvp.Key, GameManager.I.playerPosition));
    //         }
    //     }
    //     return Interactive.TopTarget(data);
    // }
    // void RemoveNullInteractives() {
    //     interactives = interactives
    //         .Where(f => f.Value != null && f.Key != null && f.Value.interactible)
    //         .ToDictionary(x => x.Key, x => x.Value);
    // }

    // void OnTriggerEnter(Collider other)
    //     => AddInteractive(other);

    // void OnTriggerExit(Collider other)
    //     => RemoveInteractive(other);

    public void SetCursorData(CursorData cursorData) {
        cursorTarget = cursorData.highlightableTargetData;
        selectedAttackSurface = cursorData.attackSurface;
        OnValueChanged?.Invoke(this);
    }
    public ItemUseResult SetInputs(PlayerInput inputs, bool gunIsHolstered) {
        bool doAction = gunIsHolstered ? inputs.actionButtonPressed || inputs.Fire.FirePressed : inputs.actionButtonPressed;
        if (doAction && (inputs.Fire.cursorData.highlightableTargetData?.targetIsInRange ?? false)) {
            Interactive cursorInteractive = cursorTarget?.target.GetComponent<Interactive>();
            if (cursorInteractive != null && cursorInteractive.interactible) {
                ItemUseResult result = cursorInteractive.DoActionAndUpdateState(this);
                // RemoveNullInteractives();
                OnValueChanged?.Invoke(this);
                if (!cursorInteractive.interactible) {
                    cursorTarget = null;
                }
                return result;
            }
            return ItemUseResult.Empty();
        } else
            return ItemUseResult.Empty();

        // if (inputs.actionButtonPressed) {
        //     InteractorTargetData data = ActiveTarget();
        //     if (data == null) return ItemUseResult.Empty();
        //     OnActionDone?.Invoke(data);
        //     return data.target.DoAction(this);
        //     // return ItemUseResult.Empty() with { waveArm = true };
        // } else {
    }

    public void HandleInteractButtonCallback(AttackSurface attackSurface) {
        if (attackSurface == null) return;
        ItemUseResult result = ItemUseResult.Empty();
        result.attackSurface = attackSurface;
        result.doBurgle = true;
        characterController.HandleItemUseResult(result);
    }
}
