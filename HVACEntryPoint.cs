using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HVACEntryPoint : Interactive {
    public HVACNetwork network;
    public HVACElement startElement;
    public override ItemUseResult DoAction(Interactor interactor) {
        ItemUseResult result = ItemUseResult.Empty() with {
            hvacUseResult = HvacUseResult.Empty() with {
                hVACNetwork = network,
                startElement = startElement,
                activateHVAC = true
            }
        };
        return result;
    }
}
