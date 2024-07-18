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

    public CharacterController characterController;

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
                OnValueChanged?.Invoke(this);
                if (result.showKeyMenu) {
                    GameManager.I.uiController.ShowKeyMenu(result.doorlocks);
                }
                if (!cursorInteractive.interactible) {
                    cursorTarget = null;
                }
                return result;
            }
            return ItemUseResult.Empty();
        } else
            return ItemUseResult.Empty();
    }

    public void HandleInteractButtonCallback(AttackSurface attackSurface) {
        if (attackSurface == null) return;
        ItemUseResult result = ItemUseResult.Empty();
        result.attackSurface = attackSurface;
        result.doBurgle = true;
        characterController.HandleItemUseResult(result);
    }
}
