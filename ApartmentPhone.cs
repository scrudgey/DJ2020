using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApartmentPhone : Interactive {

    public override ItemUseResult DoAction(Interactor interactor) {
        GameManager.I.ShowMenu(MenuType.phoneMenu);
        return ItemUseResult.Empty() with {
            waveArm = true
        };
    }
}
