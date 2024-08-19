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

        // ScriptSceneLocation mentorLockpickData = CutsceneManager.I.worldLocations["mentor_greet"];
        // mentorObject.transform.position = mentorLockpickData.transform.position;
        // mentorController.Motor.SetPosition(mentorLockpickData.transform.position, bypassInterpolation: true);
        // mentorObject.SetActive(true);

        yield return FromStartToGreet();

        yield return GreetHackSequence();

        yield return FenceAndNetworks();

    }

    IEnumerator FromStartToGreet() {

        yield return WaitForFocus();
        Time.timeScale = 1f;
        SetCameraPosition("alleystart", CameraState.normal, orthographicSize: 5f, snapToOrthographicSize: true, snapToPosition: true);

        yield return Toolbox.Parallelize(MoveCharacter(mentorController, "mentorWalk1"), MoveCharacter(GameManager.I.playerCharacterController, "playerWalk1"));
        yield return GameManager.I.uiController.ShowCutsceneDialogue("mentor", mentorPortrait, "Alright, kid, listen up. This ain't no heist. It's a stroll through the park. We're in, we're out, no alarms, no drama. Got it?");
        yield return GameManager.I.uiController.ShowCutsceneDialogue("mentor", mentorPortrait, "Our target is in a data warehouse just up ahead. I'll go ahead to scout it out, you follow behind me. Remember, eyes and ears open.");
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
        Debug.Log("conclude part 1");
    }

    IEnumerator GreetHackSequence() {
        /* greet sequence */
        Debug.Log("start part 2");

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
        yield return GameManager.I.uiController.ShowCutsceneDialogue("mentor", mentorPortrait, "Before we go poking around blind, let's see if we can't pinpoint our data file. We'll scan the network and find out exactly where it's hiding.");
        yield return new WaitForSecondsRealtime(0.25f);
        yield return LeaveFocus();
        HideBasicUI();

        Coroutine mentorWalking = CutsceneManager.I.StartCoroutine(MoveCharacter(mentorController, "mentor_kiosk", speedCoefficient: 1f));
        HighlightLocation("player_kiosk");
        yield return WaitForTrigger("kiosk");
        HideLocationHighlight();
        yield return WaitForFocus();
        GameManager.I.uiController.ShowOverlayControls(true);
        Time.timeScale = 1f;
        yield return GameManager.I.uiController.ShowCutsceneDialogue("mentor", mentorPortrait, "We should be able to tap into their network from this wallcom. Get out your cyberdeck.");
        inputProfile = inputProfile with {
            allowePlayerItemSelect = true,
            allowCameraControl = true
        };
        GameManager.I.uiController.ShowTutorialText("Hold X to open inventory menu. Select cyberdeck and release X.");

        yield return WaitForTrigger("item_select_deck");
        inputProfile = InputProfile.allowNone with {
            allowCameraControl = true
        };

        GameManager.I.uiController.HideCutsceneDialogue();
        yield return RotateIsometricCamera(IsometricOrientation.NW, playerCharacterController.transform.position);
        yield return CameraIsometricZoom(2f, 0.5f);

        GameManager.I.uiController.ShowTutorialText("Click on the wallcom node (<sprite name=\"polygon\">) to connect to it.");
        yield return WaitForTrigger("node_select_wallcom");
        GameManager.I.uiController.HideCutsceneText();

        yield return new WaitForSecondsRealtime(1.5f);
        yield return RotateIsometricCamera(IsometricOrientation.SW, playerCharacterController.transform.position);
        yield return CameraIsometricZoom(5f, 0.5f);

        yield return new WaitForSecondsRealtime(1.5f);
        yield return GameManager.I.uiController.ShowCutsceneDialogue("mentor", mentorPortrait, "Okay, we're looking at the network now. ");

        yield return new WaitForSecondsRealtime(1.2f);
        GameManager.I.uiController.DrawLine(GameManager.I.uiController.overlayHandler.cyberOverlay.GetIndicator("cyberdeck").rectTransform, Vector3.zero);
        yield return GameManager.I.uiController.ShowCutsceneDialogue("mentor", mentorPortrait, "This node here is your cyberdeck and the node it is connected to is the wallcom.");

        GameManager.I.uiController.DrawLine(GameManager.I.uiController.indicatorUIController.cyberInfoIndicatorAnchor, Vector3.zero);
        yield return GameManager.I.uiController.ShowCutsceneDialogue("mentor", mentorPortrait, "In the node display we see info about the wallcom.");

        // GameManager.I.uiController.ShowIndicator(GameManager.I.uiController.indicatorUIController.hackIndicatorAnchor, Vector3.zero, IndicatorUIController.Direction.right);
        GameManager.I.uiController.DrawLine(GameManager.I.uiController.indicatorUIController.hackIndicatorAnchor, Vector3.zero);
        yield return GameManager.I.uiController.ShowCutsceneDialogue("mentor", mentorPortrait, "From the cyberdeck interface we can launch attacks against the target.");
        GameManager.I.uiController.HideAllIndicators();

        inputProfile = inputProfile with {
            allowCameraControl = false
        };

        // TODO: zoom out
        yield return MoveCamera("router", 1f, CameraState.normal);
        yield return GameManager.I.uiController.ShowCutsceneDialogue("mentor", mentorPortrait, "We want to get up to that router to see if it can let us in the building.");
        inputProfile = inputProfile with {
            allowCameraControl = true
        };

        GameManager.I.uiController.ShowTutorialText("Click on the router node (<sprite name=\"polygon\">) to connect to it.\nQ/E and scroll wheel to control camera");
        GameManager.I.uiController.ShowIndicator(GameManager.I.uiController.overlayHandler.cyberOverlay.GetIndicator("router").rectTransform, Vector3.left * 25f, IndicatorUIController.Direction.right);

        yield return WaitForTrigger("node_select_router");
        GameManager.I.uiController.HideAllIndicators();
        GameManager.I.uiController.HideCutsceneText();
        yield return new WaitForSecondsRealtime(1f);
        yield return GameManager.I.uiController.ShowCutsceneDialogue("mentor", mentorPortrait, "Alright, this node is probably connected to the target building. We need to scan it to reveal its connections.");
        GameManager.I.uiController.ShowIndicator(GameManager.I.uiController.indicatorUIController.hackButtonIndicator, Vector3.zero, IndicatorUIController.Direction.right);
        GameManager.I.uiController.ShowTutorialText("Click on the Hack... button to open the hack software interface");
        yield return WaitForTrigger("hack_software_open");
        GameManager.I.uiController.HideAllIndicators();
        GameManager.I.uiController.HideCutsceneText();

        // TODO: disable software interface
        yield return GameManager.I.uiController.ShowCutsceneDialogue("mentor", mentorPortrait, "I loaded up your cyberdeck with some basic software. Use the scan software.\n[EDITOR'S NOTE: this whole interface will change at some point]");
        GameManager.I.uiController.ShowIndicator(GameManager.I.uiController.softwareModalController.selectorIndicators["scan"], Vector3.left * 200f, IndicatorUIController.Direction.right);
        // TODO: enable software interface

        yield return WaitForTrigger("software_deploy_scan");
        GameManager.I.uiController.HideAllIndicators();

        yield return WaitForTrigger("software_complete_scan");
        inputProfile = inputProfile with {
            allowCameraControl = false
        };
        // (Vector3 targetPosition, Quaternion targetRotation, float duration, CameraState state, Vector2 zoomInput)
        // yield return MoveCamera("router2", 0.5f, CameraState.normal);
        yield return new WaitForSecondsRealtime(0.5f);
        yield return GameManager.I.uiController.ShowCutsceneDialogue("mentor", mentorPortrait, "Bingo. There's our way in.");
        yield return new WaitForSecondsRealtime(0.5f);

        inputProfile = inputProfile with {
            allowCameraControl = true
        };

        // TODO: change node from router 2 to printer
        GameManager.I.uiController.ShowTutorialText("Click on the unknown node (<sprite name=\"question\">) to connect to it.\nQ/E and scroll wheel to control camera");
        GameManager.I.uiController.ShowIndicator(GameManager.I.uiController.overlayHandler.cyberOverlay.GetIndicator("router 2").rectTransform, Vector3.left * 25f, IndicatorUIController.Direction.right);
        yield return WaitForTrigger("node_select_router 2");
        GameManager.I.uiController.HideAllIndicators();
        GameManager.I.uiController.HideCutsceneText();

        yield return GameManager.I.uiController.ShowCutsceneDialogue("mentor", mentorPortrait, "This looks like the lab, our target data file is somewhere in here. We need to scan more, but we're too far away from your cyberdeck to use our software.");
        yield return GameManager.I.uiController.ShowCutsceneDialogue("mentor", mentorPortrait, "We'll need to take control of that router and use it to launch our attacks.");

        GameManager.I.uiController.ShowIndicator(GameManager.I.uiController.overlayHandler.cyberOverlay.GetIndicator("router").rectTransform, Vector3.left * 25f, IndicatorUIController.Direction.right);
        GameManager.I.uiController.ShowTutorialText("Click on the router node (<sprite name=\"polygon\">) to connect to it.\nQ/E and scroll wheel to control camera");
        yield return WaitForTrigger("node_select_router");
        GameManager.I.uiController.HideAllIndicators();

        GameManager.I.uiController.ShowIndicator(GameManager.I.uiController.indicatorUIController.hackButtonIndicator, Vector3.zero, IndicatorUIController.Direction.right);
        GameManager.I.uiController.ShowTutorialText("Click on the Hack... button to open the hack software interface");

        yield return WaitForTrigger("hack_software_open");
        GameManager.I.uiController.HideAllIndicators();
        GameManager.I.uiController.HideCutsceneText();

        // TODO: disable software interface
        GameManager.I.uiController.ShowIndicator(GameManager.I.uiController.softwareModalController.selectorIndicators["exploit"], Vector3.left * 200f, IndicatorUIController.Direction.right);
        // TODO: enable software interface

        yield return WaitForTrigger("software_deploy_exploit");
        GameManager.I.uiController.HideAllIndicators();
        yield return WaitForTrigger("software_complete_exploit");

        GameManager.I.uiController.ShowIndicator(GameManager.I.uiController.overlayHandler.cyberOverlay.GetIndicator("router 2").rectTransform, Vector3.left * 25f, IndicatorUIController.Direction.right);
        GameManager.I.uiController.ShowTutorialText("Click on the router node (<sprite name=\"polygon\">) to connect to it.");
        yield return WaitForTrigger("node_select_router 2");

        GameManager.I.uiController.ShowIndicator(GameManager.I.uiController.indicatorUIController.hackButtonIndicator, Vector3.zero, IndicatorUIController.Direction.right);
        GameManager.I.uiController.ShowTutorialText("Click on the Hack... button and use the scan software");
        yield return WaitForTrigger("hack_software_open");
        GameManager.I.uiController.HideAllIndicators();
        GameManager.I.uiController.HideCutsceneText();
        GameManager.I.uiController.ShowIndicator(GameManager.I.uiController.softwareModalController.selectorIndicators["scan"], Vector3.left * 200f, IndicatorUIController.Direction.right);
        yield return WaitForTrigger("software_deploy_scan");
        GameManager.I.uiController.HideAllIndicators();
        yield return WaitForTrigger("software_complete_scan");


        yield return new WaitForSecondsRealtime(0.5f);
        yield return GameManager.I.uiController.ShowCutsceneDialogue("mentor", mentorPortrait, "Just as I thought. There's three cyber nodes in there and one of them should have our data.");
        yield return GameManager.I.uiController.ShowCutsceneDialogue("mentor", mentorPortrait, "Use the data scan on each node to find which one has our target.");
        GameManager.I.uiController.ShowTutorialText("Use the data scan software on each node until you find the target (<sprite name=\"data_objective\">).");

        RectTransform superComputerTransform = GameManager.I.uiController.overlayHandler.cyberOverlay.GetIndicator("super computer").rectTransform;
        RectTransform terminalTransform = GameManager.I.uiController.overlayHandler.cyberOverlay.GetIndicator("terminal").rectTransform;
        RectTransform computerTransform = GameManager.I.uiController.overlayHandler.cyberOverlay.GetIndicator("computer").rectTransform;

        RectTransform[] transforms = new RectTransform[] {
            superComputerTransform, terminalTransform, computerTransform
        };
        Vector3[] offsets = new Vector3[] {
            Vector3.left * 25f, Vector3.left * 25f, Vector3.left * 25f,
        };
        IndicatorUIController.Direction[] directions = new IndicatorUIController.Direction[] {
            IndicatorUIController.Direction.right, IndicatorUIController.Direction.right, IndicatorUIController.Direction.right
        };
        GameManager.I.uiController.ShowIndicators(transforms, offsets, directions);

        Coroutine sdataTrigger = CutsceneManager.I.StartCoroutine(Toolbox.ChainCoroutines(
             WaitForTrigger("file_discover_scdata"),
             Toolbox.CoroutineFunc(() => {
                 Debug.Log($"file discover sdata: {superComputerTransform}");
                 GameManager.I.uiController.HideIndicator(superComputerTransform);
             })
         ));
        Coroutine cdataTrigger = CutsceneManager.I.StartCoroutine(Toolbox.ChainCoroutines(
            WaitForTrigger("file_discover_cdata"),
            Toolbox.CoroutineFunc(() => {
                Debug.Log($"file discover cdata: {computerTransform}");
                GameManager.I.uiController.HideIndicator(computerTransform);
            })
        ));

        yield return WaitForTrigger("file_discover_DAT001");
        GameManager.I.uiController.HideCutsceneText();
        GameManager.I.uiController.HideAllIndicators();
        if (sdataTrigger != null) {
            CutsceneManager.I.StopCoroutine(sdataTrigger);
        }
        if (cdataTrigger != null) {
            CutsceneManager.I.StopCoroutine(cdataTrigger);
        }

        yield return GameManager.I.uiController.ShowCutsceneDialogue("mentor", mentorPortrait, "Great work. There's our package.");
        yield return GameManager.I.uiController.ShowCutsceneDialogue("mentor", mentorPortrait, "Alright, we've got our target. You can keep snooping the net if you want. When you're ready to roll, hit the disconnect and meet me by the fence.");

        // click "DISCONNECT" button
        yield return LeaveFocus();
        HideBasicUI();
        GameManager.I.uiController.ShowOverlayControls(true);
    }

    IEnumerator FenceAndNetworks() {
        CutsceneManager.I.StartCoroutine(MoveCharacter(mentorController, "mentor_lockpick", speedCoefficient: 0.4f));
        HighlightLocation("lockpick");
        yield return WaitForTrigger("lockpick2");
        yield return WaitForFocus();
        yield return RotateIsometricCamera(IsometricOrientation.NE, playerCharacterController.transform.position);
        HideLocationHighlight();

        yield return GameManager.I.uiController.ShowCutsceneDialogue("mentor", mentorPortrait, "Alright. Did you remember to bring the fence cutters? Time to bring them out.");
        inputProfile = InputProfile.allowNone with {
            allowePlayerItemSelect = true,
        };
        GameManager.I.uiController.ShowTutorialText("Hold X to open inventory menu. Select fence cutters and release X.");

        yield return WaitForTrigger("item_select_fence_cutters");
        inputProfile = InputProfile.allowNone;

        GameManager.I.uiController.HideCutsceneDialogue();

        // yield return MoveCamera("lockpick", 1.5f, CameraState.normal, zoomInput, PennerDoubleAnimation.ExpoEaseOut);
        // yield return GameManager.I.uiController.ShowCutsceneDialogue("mentor", mentorPortrait, "Hey Jack, get over here! I need you to open this lock.");
        // yield return CutsceneManager.I.LeaveFocus(this);
        // HideBasicUI();

        // yield return WaitForTrigger("lockpick2");
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