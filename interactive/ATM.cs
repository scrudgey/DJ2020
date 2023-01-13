using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ATM : Interactive {
    public override ItemUseResult DoAction(Interactor interactor) {
        // throw new System.NotImplementedException();
        return ItemUseResult.Empty() with { waveArm = true };
    }
}
