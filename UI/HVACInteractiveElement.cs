using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HVACInteractiveElement : Interactive {
    public HVACElement element;

    public override ItemUseResult DoAction(Interactor interactor) {
        ItemUseResult result = ItemUseResult.Empty() with {
            hvacUseResult = HvacUseResult.Empty() with {
                dismountElement = element
            }
        };
        return result;
    }

    public override bool AllowInteraction() {
        return GameManager.I.playerCharacterController.state == CharacterState.hvac;
    }

}
