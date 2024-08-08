using System;
using System.Collections;
using System.Collections.Generic;
using Easings;
using UnityEngine;

class CreditsCutscene : Cutscene {

    public CreditsCutscene() {
    }

    public override IEnumerator DoCutscene() {
        yield return WaitForTrigger("credits");
        Debug.Log("credits go");
        yield return GameManager.I.uiController.FadeInCreditsText("DEVELOPER\nRyan Foltz", 2f);
        yield return new WaitForSecondsRealtime(4f);
        yield return GameManager.I.uiController.FadeOutCreditsText(2f);
        yield return new WaitForSecondsRealtime(4f);

        yield return GameManager.I.uiController.FadeInCreditsText("MUSIC\nNathan Wiswall", 2f);
        yield return new WaitForSecondsRealtime(4f);
        yield return GameManager.I.uiController.FadeOutCreditsText(2f);
    }

}