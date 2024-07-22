using System;
using System.Collections;
using System.Collections.Generic;
using Easings;
using UnityEngine;

class AbandonedLabIntroCutscene : Cutscene {
    Sprite portrait;
    public AbandonedLabIntroCutscene(Sprite portrait) {
        this.portrait = portrait;
    }

    public override IEnumerator DoCutscene() {
        yield return CutsceneManager.WaitForTrigger("labIntro");
        yield return WaitForFocus();
        yield return MoveCamera("door", 1f, state: CameraState.normal, PennerDoubleAnimation.ExpoEaseOut);
        yield return GameManager.I.uiController.ShowCutsceneDialogue("Ishikawa", portrait, "This is the place. See if you can bypass the door lock to get in.");

    }
}