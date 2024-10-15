using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AI;
using Easings;
using Items;
using UnityEditor;
using UnityEngine;

class BarCutscene : Cutscene {

    Sprite jackPortrait;
    Sprite nuisancePortrait;
    Sprite bartenderPortrait;
    NPCTemplate nuisanceTemplate;


    public static BarCutscene fromResources() {
        Sprite jackPortrait = Resources.Load<Sprite>("sprites/portraits/Jack") as Sprite;
        Sprite barTenderPortrait = Resources.Load<Sprite>("sprites/portraits/civ_male") as Sprite;
        Sprite nuisancePortrait = Resources.Load<Sprite>("sprites/portraits/NPC 11") as Sprite;
        NPCTemplate nuisanceTemplate = Resources.Load<NPCTemplate>("data/npc/guard3") as NPCTemplate;
        return new BarCutscene(jackPortrait, barTenderPortrait, nuisancePortrait, nuisanceTemplate);
    }

    public BarCutscene(Sprite jackPortrait, Sprite bartenderPortrait, Sprite nuisancePortrait, NPCTemplate nuisanceTemplate) {
        this.jackPortrait = jackPortrait;
        this.bartenderPortrait = bartenderPortrait;
        this.nuisancePortrait = nuisancePortrait;
        this.nuisanceTemplate = nuisanceTemplate;
    }

    public override IEnumerator DoCutscene() {
        CutsceneManager.I.SpawnNPC("nuisance_bar", nuisanceTemplate);
        yield return WaitForFocus();
        // yield return new WaitForSecondsRealtime(0.3f);

        Time.timeScale = 1f;
        ScriptSceneLocation barPlayerLocation = CutsceneManager.I.worldLocations["player_bar"];

        playerCharacterController.transform.position = barPlayerLocation.transform.position;
        playerCharacterController.Motor.SetPosition(barPlayerLocation.transform.position, bypassInterpolation: true);
        yield return MoveCamera("bar_exterior", 1f, CameraState.free);
        yield return new WaitForSeconds(2f);
        yield return ShowCutsceneDialogue("bartender", bartenderPortrait, "Gone. Just like that...");
        yield return new WaitForSeconds(2f);
        yield return MoveCamera("player_bar", 1f, CameraState.normal, buffer: 0.2f);
        yield return CameraIsometricZoom(2f, 0.1f);
        yield return new WaitForSecondsRealtime(1f);
        yield return ShowCutsceneDialogue("bartender", bartenderPortrait, "Everybody knew the Mentor. He helped a lot of us get on our feet.");
        yield return ShowCutsceneDialogue("jack", jackPortrait, "Just another job gone wrong.");
        yield return ShowCutsceneDialogue("jack", jackPortrait, "Shit! Why didn't I do anything? He was always one step ahead of the game, he knew all the angles...");

        yield return new WaitForSeconds(0.5f);

        yield return MoveCamera("nuisance_bar", 1f, CameraState.normal, buffer: 0.2f);
        yield return ShowCutsceneDialogue("nuisance", nuisancePortrait, "He was old.");
        yield return MoveCamera("player_bar", 1f, CameraState.normal, buffer: 0.2f);
        yield return ShowCutsceneDialogue("jack", jackPortrait, "What did you say?");
        yield return MoveCamera("nuisance_bar", 1f, CameraState.normal, buffer: 0.2f);
        yield return ShowCutsceneDialogue("nuisance", nuisancePortrait, "The Mentor was old. He got slow. Couldn't cut it on the street, not like he did in the old days.");
        yield return MoveCamera("player_bar", 1f, CameraState.normal, buffer: 0.2f);
        yield return new WaitForSeconds(0.5f);
        yield return ShowCutsceneDialogue("jack", jackPortrait, "...");
        yield return ShowCutsceneDialogue("bartender", bartenderPortrait, "Ignore him. Did you see the guys who did it?");
        yield return ShowCutsceneDialogue("jack", jackPortrait, "They looked like some security goons. But they knew he was coming, asked him about some device or something...");
        yield return ShowCutsceneDialogue("bartender", bartenderPortrait, "Pretty unusual for security to just ice someone like that. Something's not adding up here. I'd watch my back if I were you.");
        yield return ShowCutsceneDialogue("jack", jackPortrait, "Yeah, yeah...");
        yield return new WaitForSeconds(1f);
        yield return ShowCutsceneDialogue("bartender", bartenderPortrait, "Listen, you can't stay here. Get yourself somewhere safe and think it over.");
        yield return ShowCutsceneDialogue("bartender", bartenderPortrait, "You should talk to Guru Flash, he and the Mentor were tight. Maybe he'll have something for you.");
        // nuisance_bar

        // yield return ShowCutsceneDialogue("jack", jackPortrait, "What was I doing in the air ducts? I should have saved him. I had my gun....");

        yield return new WaitForSecondsRealtime(1f);

        yield return LeaveFocus();
    }
}