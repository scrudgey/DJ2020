using System;
using System.Collections;
using System.Collections.Generic;
using Easings;
using UnityEngine;

class TutorialCutscene : Cutscene {
    NPCTemplate mentorTemplate;
    GameObject mentorObject;
    CharacterController mentorController;
    Sprite mentorPortrait;
    Vector2 zoomInput = new Vector2(0f, 5f);

    public TutorialCutscene(NPCTemplate mentorTemplate, Sprite mentorPortrait) {
        this.mentorTemplate = mentorTemplate;
        this.mentorPortrait = mentorPortrait;
    }

    public override IEnumerator DoCutscene() {
        // HideBasicUI();

        mentorObject = CutsceneManager.I.SpawnNPC("mentorSpawn", mentorTemplate);
        mentorController = mentorObject.GetComponent<CharacterController>();

        ScriptSceneLocation mentorLockpickData = CutsceneManager.I.worldLocations["mentor_greet"];
        mentorObject.transform.position = mentorLockpickData.transform.position;
        mentorController.Motor.SetPosition(mentorLockpickData.transform.position, bypassInterpolation: true);
        mentorObject.SetActive(true);

        // yield return FromStartToGreet();

        yield return GreetHackSequence();

        yield return FenceAndNetworks();

    }

    IEnumerator FromStartToGreet() {

        yield return WaitForFocus();
        Time.timeScale = 1f;
        SetCameraPosition("alleystart", CameraState.normal, orthographicSize: 5f, snapToOrthographicSize: true, snapToPosition: true);

        yield return Toolbox.Parallelize(MoveCharacter(mentorController, "mentorWalk1"), MoveCharacter(GameManager.I.playerCharacterController, "playerWalk1"));
        yield return GameManager.I.uiController.ShowCutsceneDialogue("mentor", mentorPortrait, "Alright kid, this is the place. Follow my lead and we'll be in and out with the score before they know what hit them.");
        yield return GameManager.I.uiController.ShowCutsceneDialogue("mentor", mentorPortrait, "Our target is in a data warehouse just up ahead. I'll go ahead to scout it out.");
        yield return MoveCharacter(mentorController, "mentorWalk2");

        // move mentor to mentor_greet
        ScriptSceneLocation mentorLockpickData = CutsceneManager.I.worldLocations["mentor_greet"];
        mentorObject.transform.position = mentorLockpickData.transform.position;
        mentorController.Motor.SetPosition(mentorLockpickData.transform.position, bypassInterpolation: true);
        mentorObject.SetActive(true);

        yield return LeaveFocus();
        HideBasicUI();
        GameManager.I.uiController.ShowTutorialText("Movement: WASD");
        yield return WaitForTrigger("walk1");
        GameManager.I.uiController.ShowTutorialText("Rotate camera: Q / E\nZoom camera: mouse wheel");
        yield return WaitForTrigger("crawl");
        yield return WaitForFocus();
        yield return RotateIsometricCamera(IsometricOrientation.NW, playerCharacterController.transform.position);
        ScriptSceneCameraPosition crawlData = CutsceneManager.I.cameraLocations["crawl"];
        GameManager.I.uiController.ShowTutorialText("Hold Control while moving to crawl");
        yield return MoveCamera(crawlData.transform.position, crawlData.transform.rotation, 2f, CameraState.normal, zoomInput, PennerDoubleAnimation.ExpoEaseOut);
        yield return new WaitForSecondsRealtime(1f);
        yield return LeaveFocus();
        HideBasicUI();


        yield return WaitForTrigger("ladder");
        yield return WaitForFocus();
        yield return RotateIsometricCamera(IsometricOrientation.NW, playerCharacterController.transform.position);

        yield return MoveCamera("ladder", 1.5f, CameraState.normal, Vector2.zero, PennerDoubleAnimation.ExpoEaseOut);
        GameManager.I.uiController.ShowTutorialText("Click on the ladder to climb");
        yield return new WaitForSecondsRealtime(1.7f);
        yield return LeaveFocus();

        HighlightLocation("ladder");
        HideBasicUI();
        yield return WaitForTrigger("interact_ladder");
        HideLocationHighlight();
        GameManager.I.uiController.ShowTutorialText("Use W / S to climb up and down the ladder\nClimb to the top to dismount");
        yield return WaitForTrigger("ladder_dismount");
        GameManager.I.uiController.HideCutsceneText();
    }

    IEnumerator GreetHackSequence() {
        /* greet sequence */
        yield return WaitForTrigger("greet");
        yield return WaitForFocus();
        HideBasicUI();

        Time.timeScale = 1f;
        yield return MoveCharacter(playerCharacterController, "player_greet", speedCoefficient: 1f);
        CharacterLookAt(playerCharacterController, "building_look");
        CharacterLookAt(mentorController, "building_look");
        yield return MoveCamera("greet", 0.5f, CameraState.free);
        yield return new WaitForSecondsRealtime(1f);
        yield return GameManager.I.uiController.ShowCutsceneDialogue("mentor", mentorPortrait, "Alright kid, this is the place. Quiet night, all the better for us.");
        yield return GameManager.I.uiController.ShowCutsceneDialogue("mentor", mentorPortrait, "Before we go in there, let's scan the network and see if we can locate our target first.");
        yield return LeaveFocus();
        HideBasicUI();

        Coroutine mentorWalking = CutsceneManager.I.StartCoroutine(MoveCharacter(mentorController, "mentor_kiosk", speedCoefficient: 1f));
        HighlightLocation("player_kiosk");
        yield return WaitForTrigger("kiosk");
        HideLocationHighlight();
        yield return WaitForFocus();
        GameManager.I.uiController.ShowOverlayControls(true);
        Time.timeScale = 1f;
        yield return GameManager.I.uiController.ShowCutsceneDialogue("mentor", mentorPortrait, "Okay, we should be able to tap into their network from this wallcom. Get out your cyberdeck.");
        inputProfile = inputProfile with {
            allowePlayerItemSelect = true,
            allowCameraControl = true
        };
        Debug.Log("set input profile");
        GameManager.I.uiController.ShowTutorialText("Hold X to open inventory menu. Select cyberdeck and release X.");

        yield return WaitForTrigger("item_select_deck");
        inputProfile = InputProfile.allowNone with {
            allowCameraControl = true
        };

        GameManager.I.uiController.HideCutsceneDialogue();

        // yield return GameManager.I.uiController.ShowCutsceneDialogue("mentor", mentorPortrait, "Good. Let's connect to the wallcom.");
        GameManager.I.uiController.ShowTutorialText("Click on the wallcom node (icon) to connect to it.");

        yield return WaitForTrigger("node_select_wallcom");
        inputProfile = InputProfile.allowNone;
        
    }

    IEnumerator FenceAndNetworks() {

        // yield return WaitForTrigger("lockpick");
        // yield return WaitForFocus();
        // yield return RotateIsometricCamera(IsometricOrientation.NE, playerCharacterController.transform.position);
        // yield return MoveCamera("lockpick", 1.5f, CameraState.normal, zoomInput, PennerDoubleAnimation.ExpoEaseOut);
        // yield return GameManager.I.uiController.ShowCutsceneDialogue("mentor", mentorPortrait, "Hey Jack, get over here! I need you to open this lock.");
        // HighlightLocation("lockpick");
        // yield return CutsceneManager.I.LeaveFocus(this);
        // HideBasicUI();

        // yield return WaitForTrigger("lockpick2");
        // HideLocationHighlight();
        // GameManager.I.uiController.ShowTutorialText("Mouse over the door and click on the <sprite name=\"screwdriver\"> icon to open the burglar view");

        // yield return WaitForTrigger("burglar_open");
        // GameManager.I.uiController.tutorialBurglarInterrupt = true;
        // yield return new WaitForSecondsRealtime(0.75f);
        // yield return GameManager.I.uiController.ShowCutsceneDialogue("mentor", mentorPortrait, "Okay, this is the burglar interface. Let's get your lockpick out.");
        // GameManager.I.uiController.tutorialBurglarInterrupt = false;
        // GameManager.I.uiController.tutorialBurglarAwaitLockpick = true;


        // yield return WaitForTrigger("burglartool_lockpick");
        // yield return new WaitForSecondsRealtime(0.1f);
        // GameManager.I.uiController.tutorialBurglarInterrupt = true;
        // yield return GameManager.I.uiController.ShowCutsceneDialogue("mentor", mentorPortrait, "Great. Now use it on the lock. Hold the mouse button until lockpicking is finished.");
        // GameManager.I.uiController.tutorialBurglarInterrupt = false;
        // GameManager.I.uiController.tutorialBurglarAwaitLockpick = false;

        // // await lock picked
        // yield return WaitForTrigger("lock_picked");
        // yield return new WaitForSecondsRealtime(1.5f);
        // GameManager.I.CloseBurglar();
        // yield return new WaitForSecondsRealtime(0.1f);
        // yield return WaitForFocus();
        // Time.timeScale = 1f;


        yield return MoveCharacter(mentorController, "hide1", speedCoefficient: 0.4f);
        yield return MoveCharacter(playerCharacterController, "hide2", speedCoefficient: 0.4f);
        yield return new WaitForSecondsRealtime(0.75f);
        yield return GameManager.I.uiController.ShowCutsceneDialogue("mentor", mentorPortrait, "Okay, careful now. See that camera over there?");
        yield return MoveCamera("camera", 1.5f, CameraState.normal, Vector2.zero, PennerDoubleAnimation.ExpoEaseOut);
        yield return new WaitForSecondsRealtime(0.75f);
        yield return GameManager.I.uiController.ShowCutsceneDialogue("mentor", mentorPortrait, "It's pointing right at the door we want. If we move in now, they'll spot us right away.");
        yield return GameManager.I.uiController.ShowCutsceneDialogue("mentor", mentorPortrait, "Let's scope out the situation a bit. Turn on your alarm network display.");
        GameManager.I.uiController.ShowOverlayControls(true);
        GameManager.I.uiController.ShowOverlayButtonHighlight(true);
        yield return WaitForTrigger("overlay_change_3");
        GameManager.I.uiController.ShowOverlayButtonHighlight(false);
        yield return new WaitForSecondsRealtime(0.25f);
        GameManager.I.uiController.SetOverlayButtonInteractible(false);


        yield return GameManager.I.uiController.ShowCutsceneDialogue("mentor", mentorPortrait, "Just as I thought. It's wired right in to the central alarm system. If it spots us, security will be on us quick.");
        yield return GameManager.I.uiController.ShowCutsceneDialogue("mentor", mentorPortrait, "There might be an easy way to take it out. Change your overlay to the power network.");
        GameManager.I.uiController.SetOverlayButtonInteractible(true);
        GameManager.I.uiController.ShowOverlayButtonHighlight(true);
        yield return WaitForTrigger("overlay_change_1");
        GameManager.I.uiController.ShowOverlayButtonHighlight(false);
        GameManager.I.uiController.SetOverlayButtonInteractible(false);

        yield return new WaitForSecondsRealtime(0.75f);
        yield return MoveCamera("powerbox", 1.5f, CameraState.normal, Vector2.zero, PennerDoubleAnimation.ExpoEaseOut);
        yield return GameManager.I.uiController.ShowCutsceneDialogue("mentor", mentorPortrait, "It's getting power from that power relay. Take it out and the camera will shut down. Let's move.");

        yield return LeaveFocus();
        GameManager.I.uiController.SetOverlayButtonInteractible(true);

        GameManager.I.uiController.ShowStatusBar(false);
        GameManager.I.uiController.ShowAppearanceInfo(false);
        GameManager.I.uiController.ShowVisibilityInfo(false);

        HighlightLocation("power");

        CutsceneManager.I.StartCoroutine(MoveCharacter(mentorController, "mentor_power", speedCoefficient: 0.4f));
        yield return WaitForTrigger("power");
        GameManager.I.uiController.ShowTutorialText("Mouse over the power relay and click on the <sprite name=\"screwdriver\"> icon to open the burglar view");
        HideLocationHighlight();

        yield return WaitForTrigger("burglar_open");
        inputProfile.allowBurglarInterface = false;
        yield return new WaitForSecondsRealtime(0.75f);
        yield return GameManager.I.uiController.ShowCutsceneDialogue("mentor", mentorPortrait, "Alright. Use your screwdriver to remove those screws.");
        // GameManager.I.uiController.tutorialBurglarInterrupt = false;
        inputProfile.allowBurglarInterface = true;
    }

    void HideBasicUI() {
        GameManager.I.uiController.ShowStatusBar(false);
        GameManager.I.uiController.ShowAppearanceInfo(false);
        GameManager.I.uiController.ShowVisibilityInfo(false);
        GameManager.I.uiController.ShowOverlayControls(false);
    }
}