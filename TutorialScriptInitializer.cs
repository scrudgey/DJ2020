using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialScriptInitializer : MonoBehaviour {

    public NPCTemplate mentorTemplate;
    public Sprite mentorPortrait;
    bool scriptStarted;

    public void StartCutscene() {
        if (scriptStarted) {
            return;
        }
        CutsceneManager.I.StartCutscene(new TutorialCutscene(mentorTemplate, mentorPortrait));
        CutsceneManager.I.StartCutscene(new CreditsCutscene());
        MusicController.I.PlaySimpleTrack(MusicTrack.sympatheticDetonation);
        scriptStarted = true;
    }
}
