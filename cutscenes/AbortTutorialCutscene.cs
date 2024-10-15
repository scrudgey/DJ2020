using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AI;
using Easings;
using Items;
using UnityEditor;
using UnityEngine;

class AbortTutorialCutscene : Cutscene {
    Sprite jackPortrait;

    public AbortTutorialCutscene(Sprite jackportrait) {
        this.jackPortrait = jackportrait;
    }
    public override IEnumerator DoCutscene() {
        yield return WaitForFocus();
        yield return ShowCutsceneDialogue("jack", jackPortrait, "On second thought, a life of crime is not for me. I will change my ways.");
        yield return new WaitForSecondsRealtime(1f);
        GameManager.I.ReturnToTitleScreen();

    }
}