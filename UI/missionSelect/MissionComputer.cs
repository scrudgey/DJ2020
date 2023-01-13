using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionComputer : Interactive {
    public override ItemUseResult DoAction(Interactor interactor) {
        GameManager.I.ShowMissionSelectMenu();
        return ItemUseResult.Empty() with { waveArm = true };
    }
    public override string ResponseString() {
        return $"accessing mission computer...";
    }
}
