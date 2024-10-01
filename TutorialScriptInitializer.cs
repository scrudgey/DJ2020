using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialScriptInitializer : MonoBehaviour {

    public NPCTemplate mentorTemplate;
    public Sprite mentorPortrait;
    public Sprite jackPortrait;
    public GameObject vehicleObject;
    public Light spotlight;
    public NPCTemplate guardTemplate;
    public Sprite guardPortrait;
    public GameObject rainParticles;
    public AmbientZone[] rainAmbience;
    public Door closetDoor;
    bool scriptStarted;

    public void StartCutscene() {
        if (scriptStarted) {
            return;
        }
        CutsceneManager.I.StartCutscene(new TutorialCutscene(
            mentorTemplate,
            mentorPortrait,
            jackPortrait,
            vehicleObject,
            spotlight,
            guardTemplate,
            guardPortrait,
            rainParticles,
            rainAmbience,
            closetDoor));
        CutsceneManager.I.StartCutscene(new CreditsCutscene());
        MusicController.I.PlaySimpleTrack(MusicTrack.sympatheticDetonation);
        scriptStarted = true;
    }
}
