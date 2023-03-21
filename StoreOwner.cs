using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum StoreType { none, gun, item, loot, bar }
public class StoreOwner : Interactive {
    public StoreType storeType;
    public Transform lookAtPoint;
    public CharacterController characterController;
    public LootBuyerData lootBuyerData;
    void Update() {
        PlayerInput playerInput = new PlayerInput {
            lookAtPosition = lookAtPoint.position,
            snapToLook = true,
            Fire = PlayerInput.FireInputs.none
        };
        characterController.SetInputs(playerInput);
    }
    public override ItemUseResult DoAction(Interactor interactor) {
        GameManager.I.ShowShopMenu(storeType, lootBuyerData);
        return ItemUseResult.Empty() with {
            waveArm = true
        };
    }
    public override string ResponseString() {
        return $"store";
    }
}
