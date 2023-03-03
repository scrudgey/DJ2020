using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum StoreType { none, gun, item }
public class StoreOwner : Interactive {
    public StoreType storeType;
    public Transform lookAtPoint;
    public CharacterController characterController;
    void Update() {
        PlayerInput playerInput = new PlayerInput {
            lookAtPosition = lookAtPoint.position,
            snapToLook = true,
            Fire = PlayerInput.FireInputs.none
        };
        characterController.SetInputs(playerInput);
    }
    public override ItemUseResult DoAction(Interactor interactor) {
        switch (storeType) {
            case StoreType.gun:
                GameManager.I.ShowGunShopMenu();
                break;
            case StoreType.item:
                GameManager.I.ShowItemShopMenu();
                break;
        }
        return ItemUseResult.Empty() with {
            waveArm = true
        };
    }
    public override string ResponseString() {
        return $"store";
    }
}
