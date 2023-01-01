using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionComputer : Interactive {
    public override void DoAction(Interactor interactor) {
        GameManager.I.ShowMissionSelectMenu();
    }
    public override string ResponseString() {
        return $"accessing mission computer...";
    }
}
