using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Easings;
using UnityEngine;

class TutorialCutscene : Cutscene {
    NPCTemplate mentorTemplate;
    GameObject mentorObject;
    CharacterController mentorController;
    Sprite mentorPortrait;
    Vector2 zoomInput = new Vector2(0f, 5f);
    bool hasKey = false;


    public TutorialCutscene(NPCTemplate mentorTemplate, Sprite mentorPortrait) {
        this.mentorTemplate = mentorTemplate;
        this.mentorPortrait = mentorPortrait;
    }

    public override IEnumerator DoCutscene() {
        // HideBasicUI();

        mentorObject = CutsceneManager.I.SpawnNPC("mentorSpawn", mentorTemplate);
        mentorController = mentorObject.GetComponent<CharacterController>();
        SphereRobotAI mentorAI = mentorObject.GetComponent<SphereRobotAI>();
        mentorAI.enabled = false;

        // ScriptSceneLocation mentorLockpickData = CutsceneManager.I.worldLocations["mentor_greet"];
        // mentorObject.transform.position = mentorLockpickData.transform.position;
        // mentorController.Motor.SetPosition(mentorLockpickData.transform.position, bypassInterpolation: true);
        // mentorObject.SetActive(true);

        ScriptSceneLocation mentorLockpickData = CutsceneManager.I.worldLocations["mentor_power"];
        mentorObject.transform.position = mentorLockpickData.transform.position;
        mentorController.Motor.SetPosition(mentorLockpickData.transform.position, bypassInterpolation: true);
        mentorObject.SetActive(true);


        // yield return FromStartToGreet();

        // yield return GreetHackSequence();

        // yield return Fence();

        // yield return PowerHack();

        // yield return EnterDoor();
        yield return new WaitForSecondsRealtime(1);
        yield return Interior();

        yield return Laboratory();

        yield return null;
    }

    IEnumerator FromStartToGreet() {

        yield return WaitForFocus();
        Time.timeScale = 1f;
        SetCameraPosition("alleystart", CameraState.normal, orthographicSize: 5f, snapToOrthographicSize: true, snapToPosition: true);

        yield return Toolbox.Parallelize(MoveCharacter(mentorController, "mentorWalk1"), MoveCharacter(GameManager.I.playerCharacterController, "playerWalk1"));
        yield return GameManager.I.uiController.ShowCutsceneDialogue("the mentor", mentorPortrait, "Alright, kid, listen up. This ain't no heist. It's a stroll in the park. We're in, we're out, no alarms, no drama. Got it?");
        yield return GameManager.I.uiController.ShowCutsceneDialogue("the mentor", mentorPortrait, "Our target is in a data warehouse just down this alley. I'll go ahead to scout it out, you follow behind me. Remember, eyes and ears open.");
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
        yield return GameManager.I.uiController.ShowCutsceneDialogue("the mentor", mentorPortrait, "Alright kid, this is the place. Quiet night, all the better for us.");
        yield return GameManager.I.uiController.ShowCutsceneDialogue("the mentor", mentorPortrait, "Before we go poking around blind, let's see if we can't pinpoint our data file. We'll scan the network and find out exactly where it's hiding.");
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
        yield return GameManager.I.uiController.ShowCutsceneDialogue("the mentor", mentorPortrait, "We should be able to tap into their network from this wallcom. Get out your cyberdeck.");
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

        GameManager.I.uiController.ShowTutorialText("Click the wallcom node (<sprite name=\"polygon\">) to connect to it.");
        yield return WaitForTrigger("node_select_wallcom");
        GameManager.I.uiController.HideCutsceneText();

        yield return new WaitForSecondsRealtime(1.5f);
        yield return RotateIsometricCamera(IsometricOrientation.SW, playerCharacterController.transform.position);
        yield return CameraIsometricZoom(5f, 0.5f);

        yield return new WaitForSecondsRealtime(1.5f);
        yield return GameManager.I.uiController.ShowCutsceneDialogue("the mentor", mentorPortrait, "Okay, we're looking at the network now. ");

        yield return new WaitForSecondsRealtime(1.2f);
        GameManager.I.uiController.DrawLine(GameManager.I.uiController.overlayHandler.cyberOverlay.GetIndicator("cyberdeck").rectTransform, Vector3.zero);
        yield return GameManager.I.uiController.ShowCutsceneDialogue("the mentor", mentorPortrait, "This node here is your cyberdeck and the node it is connected to is the wallcom.");

        GameManager.I.uiController.DrawLine(GameManager.I.uiController.indicatorUIController.cyberInfoIndicatorAnchor, Vector3.zero);
        yield return GameManager.I.uiController.ShowCutsceneDialogue("the mentor", mentorPortrait, "This display shows info about your current target."); // TODO

        // GameManager.I.uiController.ShowIndicator(GameManager.I.uiController.indicatorUIController.hackIndicatorAnchor, Vector3.zero, IndicatorUIController.Direction.right);
        GameManager.I.uiController.DrawLine(GameManager.I.uiController.indicatorUIController.hackIndicatorAnchor, Vector3.zero);
        yield return GameManager.I.uiController.ShowCutsceneDialogue("the mentor", mentorPortrait, "Your cyberdeck is how you launch attacks against the target."); // TODO
        GameManager.I.uiController.HideAllIndicators();

        inputProfile = inputProfile with {
            allowCameraControl = false
        };

        // TODO: zoom out
        yield return MoveCamera("router", 1f, CameraState.normal);
        yield return GameManager.I.uiController.ShowCutsceneDialogue("the mentor", mentorPortrait, "We want to get up to that router to see if it can let us in the building.");
        inputProfile = inputProfile with {
            allowCameraControl = true
        };

        GameManager.I.uiController.ShowTutorialText("Click the router node (<sprite name=\"polygon\">) to connect to it.\nQ/E and scroll wheel to control camera");
        GameManager.I.uiController.ShowIndicator(GameManager.I.uiController.overlayHandler.cyberOverlay.GetIndicator("router").rectTransform, Vector3.left * 25f, IndicatorUIController.Direction.right);

        yield return WaitForTrigger("node_select_router");
        GameManager.I.uiController.HideAllIndicators();
        GameManager.I.uiController.HideCutsceneText();
        yield return new WaitForSecondsRealtime(1f);
        yield return GameManager.I.uiController.ShowCutsceneDialogue("the mentor", mentorPortrait, "Alright, this node is probably connected to the target building. We need to scan it to reveal its connections.");
        GameManager.I.uiController.ShowIndicator(GameManager.I.uiController.indicatorUIController.hackButtonIndicator, Vector3.zero, IndicatorUIController.Direction.right);
        GameManager.I.uiController.ShowTutorialText("Click the Hack... button to open the hack software interface");
        yield return WaitForTrigger("hack_software_open");
        GameManager.I.uiController.HideAllIndicators();
        GameManager.I.uiController.HideCutsceneText();

        // TODO: disable software interface
        yield return GameManager.I.uiController.ShowCutsceneDialogue("the mentor", mentorPortrait, "I loaded up your cyberdeck with some basic software. Use the scan software.\n[EDITOR'S NOTE: this whole interface will change at some point]");
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
        // yield return RotateIsometricCamera(IsometricOrientation.NE, "router2")
        yield return new WaitForSecondsRealtime(0.5f);
        yield return GameManager.I.uiController.ShowCutsceneDialogue("the mentor", mentorPortrait, "Bingo. There's our way in.");
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

        yield return GameManager.I.uiController.ShowCutsceneDialogue("the mentor", mentorPortrait, "This looks like the lab, our target data file is somewhere in here. We need to scan more, but we're too far away from your cyberdeck to use our software.");
        yield return GameManager.I.uiController.ShowCutsceneDialogue("the mentor", mentorPortrait, "We'll need to take control of that router and use it to launch our attacks.");

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
        yield return GameManager.I.uiController.ShowCutsceneDialogue("the mentor", mentorPortrait, "Just as I thought. There's three cyber nodes in there and one of them should have our data.");
        yield return GameManager.I.uiController.ShowCutsceneDialogue("the mentor", mentorPortrait, "Use the data scan on each node to find which one has our target.");
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

        yield return GameManager.I.uiController.ShowCutsceneDialogue("the mentor", mentorPortrait, "Great work. There's our package.");
        yield return GameManager.I.uiController.ShowCutsceneDialogue("the mentor", mentorPortrait, "Alright, we've got our target. You can keep snooping the net if you want. When you're ready to roll, hit the disconnect and meet me by the fence.");

        // click "DISCONNECT" button
        yield return LeaveFocus();
        HideBasicUI();
        GameManager.I.uiController.ShowOverlayControls(true);
    }

    IEnumerator Fence() {
        CutsceneManager.I.StartCoroutine(MoveCharacter(mentorController, "mentor_lockpick", speedCoefficient: 0.4f));
        HighlightLocation("lockpick");
        yield return WaitForTrigger("lockpick2");
        yield return WaitForFocus();
        Time.timeScale = 1;
        yield return RotateIsometricCamera(IsometricOrientation.NE, playerCharacterController.transform.position);
        HideLocationHighlight();
        // yield return new WaitForSecondsRealtime(1f);
        yield return MoveCharacter(playerCharacterController, "lockpick", 0.5f);
        yield return CameraIsometricZoom(2f, 0.5f);

        yield return GameManager.I.uiController.ShowCutsceneDialogue("the mentor", mentorPortrait, "Alright. Did you remember to bring the fence cutters? Time to bring them out.");
        inputProfile = InputProfile.allowNone with {
            allowePlayerItemSelect = true,
            allowCameraControl = true
        };
        GameManager.I.uiController.ShowTutorialText("Hold X to open inventory menu. Select fence cutters and release X.");

        yield return WaitForTrigger("item_select_fence cutters");
        inputProfile = InputProfile.allowNone;
        GameManager.I.uiController.HideCutsceneDialogue();
        yield return GameManager.I.uiController.ShowCutsceneDialogue("the mentor", mentorPortrait, "Perfect. Now let's make ourselves a door.");
        // TODO: health bar
        inputProfile = InputProfile.allowNone with {
            allowCameraControl = true,
            allowPlayerFireInput = true
        };
        GameManager.I.uiController.ShowTutorialText("With the fence cutters selected, click on the fence to cut.");
        yield return WaitForTrigger("fence cut fence_wall");
        inputProfile = InputProfile.allowNone;

        GameManager.I.uiController.HideCutsceneDialogue();
        yield return new WaitForSecondsRealtime(0.5f);
        yield return GameManager.I.uiController.ShowCutsceneDialogue("the mentor", mentorPortrait, "Let's go. Follow me.");
        GameManager.I.disablePlayerInput = true;
        yield return MoveCharacter(mentorController, "crawlpoint1", crawling: true);
        yield return MoveCharacter(mentorController, "hide1", speedCoefficient: 0.6f);
        yield return MoveCamera("hide", 1f, CameraState.normal);

        SelectItem(playerCharacterController);
        yield return MoveCharacter(playerCharacterController, "crawlpoint1", crawling: true);
        yield return MoveCharacter(playerCharacterController, "hide2", speedCoefficient: 0.6f);
        GameManager.I.disablePlayerInput = false;

        yield return new WaitForSecondsRealtime(0.75f);
        yield return GameManager.I.uiController.ShowCutsceneDialogue("the mentor", mentorPortrait, "Okay, careful now. See that camera over there?");
        yield return MoveCamera("camera", 1.5f, CameraState.normal, Vector2.zero, PennerDoubleAnimation.ExpoEaseOut);
        yield return CameraIsometricZoom(4.5f, 0.5f);
        yield return new WaitForSecondsRealtime(0.75f);

        yield return GameManager.I.uiController.ShowCutsceneDialogue("the mentor", mentorPortrait, "It's pointing right at the door we want. If we move in now, we'll be spotted for sure.");
        yield return GameManager.I.uiController.ShowCutsceneDialogue("the mentor", mentorPortrait, "Let's scope out the situation before we make a move. Switch to the alarm network display.");

        GameManager.I.uiController.ShowOverlayControls(true);
        GameManager.I.uiController.ShowOverlayButtonHighlight(true);
        yield return WaitForTrigger("overlay_change_3");
        GameManager.I.uiController.ShowOverlayButtonHighlight(false);
        yield return new WaitForSecondsRealtime(0.25f);
        GameManager.I.uiController.SetOverlayButtonInteractible(false);
        yield return MoveCamera("camera", 0.25f, CameraState.normal, Vector2.zero, PennerDoubleAnimation.ExpoEaseOut);

        yield return GameManager.I.uiController.ShowCutsceneDialogue("the mentor", mentorPortrait, "Just as I thought. It's wired right in to the central alarm system. If it spots us, security will be on us quick.");
        yield return GameManager.I.uiController.ShowCutsceneDialogue("the mentor", mentorPortrait, "There might be an easy way to take it out. Change your overlay to the power network.");
        GameManager.I.uiController.SetOverlayButtonInteractible(true);
        GameManager.I.uiController.ShowOverlayButtonHighlight(true);
        yield return WaitForTrigger("overlay_change_1");
        GameManager.I.uiController.ShowOverlayButtonHighlight(false);
        GameManager.I.uiController.SetOverlayButtonInteractible(false);

        yield return new WaitForSecondsRealtime(0.75f);
        yield return MoveCamera("powerbox", 1.5f, CameraState.normal, Vector2.zero, PennerDoubleAnimation.ExpoEaseOut);
        yield return GameManager.I.uiController.ShowCutsceneDialogue("the mentor", mentorPortrait, "If we cut the power from that relay, the camera will be out of the picture. Let's go.");

        // TODO: close overlay
        yield return LeaveFocus();
        GameManager.I.uiController.SetOverlayButtonInteractible(true);
        GameManager.I.uiController.ShowStatusBar(false);
        GameManager.I.uiController.ShowAppearanceInfo(false);
        GameManager.I.uiController.ShowVisibilityInfo(false);





        // yield return MoveCamera("lockpick", 1.5f, CameraState.normal, zoomInput, PennerDoubleAnimation.ExpoEaseOut);
        // yield return GameManager.I.uiController.ShowCutsceneDialogue("the mentor", mentorPortrait, "Hey Jack, get over here! I need you to open this lock.");
        // yield return CutsceneManager.I.LeaveFocus(this);
        // HideBasicUI();

        // yield return WaitForTrigger("lockpick2");
        // GameManager.I.uiController.ShowTutorialText("Mouse over the door and click on the <sprite name=\"screwdriver\"> icon to open the burglar view");

        // yield return WaitForTrigger("burglar_open");
        // GameManager.I.uiController.tutorialBurglarInterrupt = true;
        // yield return new WaitForSecondsRealtime(0.75f);
        // yield return GameManager.I.uiController.ShowCutsceneDialogue("the mentor", mentorPortrait, "Okay, this is the burglar interface. Let's get your lockpick out.");
        // GameManager.I.uiController.tutorialBurglarInterrupt = false;
        // GameManager.I.uiController.tutorialBurglarAwaitLockpick = true;


        // yield return WaitForTrigger("burglartool_lockpick");
        // yield return new WaitForSecondsRealtime(0.1f);
        // GameManager.I.uiController.tutorialBurglarInterrupt = true;
        // yield return GameManager.I.uiController.ShowCutsceneDialogue("the mentor", mentorPortrait, "Great. Now use it on the lock. Hold the mouse button until lockpicking is finished.");
        // GameManager.I.uiController.tutorialBurglarInterrupt = false;
        // GameManager.I.uiController.tutorialBurglarAwaitLockpick = false;

        // // await lock picked
        // yield return WaitForTrigger("lock_picked");
        // yield return new WaitForSecondsRealtime(1.5f);
        // GameManager.I.CloseBurglar();
        // yield return new WaitForSecondsRealtime(0.1f);
        // yield return WaitForFocus();
        // Time.timeScale = 1f;
    }

    IEnumerator PowerHack() {

        Coroutine wanderPrevention = CutsceneManager.I.StartCoroutine(Toolbox.RunJobRepeatedly(() => WanderPrevention()));

        BurglarCanvasController burglarCanvas = null;

        while (burglarCanvas == null) {
            burglarCanvas = GameObject.FindObjectOfType<BurglarCanvasController>(true);
            yield return null;
        }

        HighlightLocation("power");
        CutsceneManager.I.StartCoroutine(MoveCharacter(mentorController, "mentor_power", speedCoefficient: 0.4f));
        yield return WaitForTrigger("power");
        yield return WaitForFocus();
        burglarCanvas.PreventClose(true);
        GameManager.I.uiController.ShowInteractiveHighlight();
        Time.timeScale = 1;
        inputProfile = InputProfile.allowNone with {
            allowCameraControl = true,
            allowPlayerFireInput = true
        };
        GameManager.I.uiController.ShowTutorialText("Mouse over the power relay and click on the <sprite name=\"screwdriver\"> icon to open the burglar view");
        HideLocationHighlight();
        yield return WaitForTrigger("burglar_open");
        burglarCanvas.PreventButtons(true);
        inputProfile = InputProfile.allowNone;
        yield return new WaitForSecondsRealtime(1f);
        yield return GameManager.I.uiController.ShowCutsceneDialogue("the mentor", mentorPortrait, "Alright kid, this is your basic burglar toolkit.", location: CutsceneDialogueController.Location.top);

        GameManager.I.uiController.DrawLine(GameManager.I.uiController.indicatorUIController.probeIndicator, Vector3.zero, IndicatorUIController.Origin.bottom);

        yield return GameManager.I.uiController.ShowCutsceneDialogue("the mentor", mentorPortrait, "This is a probe / decoder. It can bypass door latches and decode combination padlocks.", location: CutsceneDialogueController.Location.top);

        // GameManager.I.uiController.DrawLine(GameManager.I.uiController.indicatorUIController.screwdriverIndicator, Vector3.zero, IndicatorUIController.Origin.bottom);

        // yield return GameManager.I.uiController.ShowCutsceneDialogue("the mentor", mentorPortrait, "This is a basic screwdriver for removing screws.", location: CutsceneDialogueController.Location.top);


        GameManager.I.uiController.DrawLine(GameManager.I.uiController.indicatorUIController.lockpickIndicator, Vector3.zero, IndicatorUIController.Origin.bottom);

        yield return GameManager.I.uiController.ShowCutsceneDialogue("the mentor", mentorPortrait, "This the lockpick. It can unlock tumbler locks that use a physical key.", location: CutsceneDialogueController.Location.top);

        GameManager.I.uiController.DrawLine(GameManager.I.uiController.indicatorUIController.wirecuttersIndicator, Vector3.zero, IndicatorUIController.Origin.bottom);

        yield return GameManager.I.uiController.ShowCutsceneDialogue("the mentor", mentorPortrait, "The wirecutters are self-explanatory. I wager we'll need them in a minute.", location: CutsceneDialogueController.Location.top);

        GameManager.I.uiController.DrawLine(GameManager.I.uiController.indicatorUIController.screwdriverIndicator, Vector3.zero, IndicatorUIController.Origin.bottom);

        yield return GameManager.I.uiController.ShowCutsceneDialogue("the mentor", mentorPortrait, "Now use this screwdriver to remove the screws on this panel.", location: CutsceneDialogueController.Location.top);
        GameManager.I.uiController.ShowTutorialText("Click on the screwdriver to select it, then click on the screws to remove them");


        inputProfile.allowBurglarInterface = true;
        burglarCanvas.PreventButtons(false);

        inputProfile = InputProfile.allowAll;

        yield return WaitForTrigger("unscrew");
        yield return WaitForTrigger("unscrew");
        inputProfile.allowBurglarInterface = false;
        burglarCanvas.PreventButtons(true);
        yield return new WaitForSecondsRealtime(0.5f);
        GameManager.I.uiController.HideCutsceneText();
        // GameManager.I.uiController.DrawLine(GameManager.I.uiController.indicatorUIController.handButtonIndicator, Vector3.zero);
        yield return GameManager.I.uiController.ShowCutsceneDialogue("the mentor", mentorPortrait, "Great, now put the screwdriver away.");
        inputProfile.allowBurglarInterface = true;
        burglarCanvas.PreventButtons(false);

        GameManager.I.uiController.ShowTutorialText("Press escape or click the hand button <sprite name=\"hand\"> to put the screwdriver away.");
        yield return WaitForTrigger("burglartool_none");
        GameManager.I.uiController.HideCutsceneText();

        inputProfile.allowBurglarInterface = false;
        burglarCanvas.PreventButtons(true);

        yield return new WaitForSecondsRealtime(0.5f);

        RectTransform panelElement = GameObject.FindObjectsOfType<AttackSurfaceUIElement>()
                    .Where(element => element.elementName == "panel").Select(element => element.GetComponent<RectTransform>()).First();
        GameManager.I.uiController.DrawLine(panelElement, Vector3.zero);
        yield return GameManager.I.uiController.ShowCutsceneDialogue("the mentor", mentorPortrait, "Now let's pop that panel open and see what we can see.");
        inputProfile.allowBurglarInterface = true;
        burglarCanvas.PreventButtons(false);


        GameManager.I.uiController.ShowTutorialText("With hand tool selected, click on the panel to open it");
        yield return WaitForTrigger("panel_open");
        GameManager.I.uiController.HideCutsceneText();

        inputProfile.allowBurglarInterface = false;
        burglarCanvas.PreventButtons(true);

        yield return new WaitForSecondsRealtime(0.25f);
        yield return GameManager.I.uiController.ShowCutsceneDialogue("the mentor", mentorPortrait, "Beautiful. The power circuitry is wide open, no hardening at all. Let's take a closer look.");
        inputProfile.allowBurglarInterface = true;
        burglarCanvas.PreventButtons(false);

        GameManager.I.uiController.ShowTutorialText("With hand tool selected, click on the circuit panel to inspect");
        yield return WaitForTrigger("circuit_view");
        GameManager.I.uiController.HideCutsceneText();
        GameObject.FindObjectOfType<BurglarCanvasController>(true).ShowReturnButton(false);

        inputProfile.allowBurglarInterface = false;
        burglarCanvas.PreventButtons(true);


        yield return new WaitForSecondsRealtime(1f);

        yield return GameManager.I.uiController.ShowCutsceneDialogue("the mentor", mentorPortrait, "Okay kid, at this point it's a sitting duck. See those wires? One of them's connected to the camera and the other goes to the main power source.");
        yield return GameManager.I.uiController.ShowCutsceneDialogue("the mentor", mentorPortrait, "All you have to do is cut one of the wires, that will be enough to bring the camera down.");
        inputProfile.allowBurglarInterface = true;
        burglarCanvas.PreventButtons(false);

        // TODO: disable "return button"
        GameManager.I.uiController.ShowTutorialText("Use the wirecutters to cut one of the green wires");


        yield return WaitForTrigger("wire_cut");
        if (wanderPrevention != null) CutsceneManager.I.StopCoroutine(wanderPrevention);

        inputProfile.allowBurglarInterface = false;
        burglarCanvas.PreventButtons(true);

        yield return new WaitForSecondsRealtime(0.5f);
        yield return GameManager.I.uiController.ShowCutsceneDialogue("the mentor", mentorPortrait, "Perfect. You've got a knack for this. Let's clean up and get out of here.");
        inputProfile.allowBurglarInterface = true;
        burglarCanvas.PreventButtons(false);


        GameManager.I.uiController.ShowTutorialText("Press escape to leave the burglar view");
        burglarCanvas.PreventClose(false);

        yield return WaitForTrigger("burglar_close");
        HideBasicUI();
        inputProfile = InputProfile.allowNone;
        yield return new WaitForSecondsRealtime(0.25f);
        yield return CameraIsometricZoom(4f, 0.5f);
        yield return MoveCamera("camera2", 1.5f, CameraState.normal, Vector2.zero, PennerDoubleAnimation.ExpoEaseOut);
        yield return GameManager.I.uiController.ShowCutsceneDialogue("the mentor", mentorPortrait, "Excellent work. The camera's offline. Now, let's move quickly. The longer we're in here, the higher the risk of someone noticing. Let's grab the data and get out.");
        yield return LeaveFocus();
        HideBasicUI();
    }
    IEnumerator WanderPrevention() {
        yield return WaitForTrigger("wander");
        yield return WaitForFocus();
        Time.timeScale = 1f;
        yield return GameManager.I.uiController.ShowCutsceneDialogue("the mentor", mentorPortrait, "Hey! What did I tell you? Get back over here!");
        yield return MoveCharacter(playerCharacterController, "wanderback", speedCoefficient: 1.2f);
        yield return GameManager.I.uiController.ShowCutsceneDialogue("the mentor", mentorPortrait, "You go wandering off like that and it could blow the whole mission! Now pay attention!");
        yield return LeaveFocus();
    }
    IEnumerator EnterDoor() {
        BurglarCanvasController burglarCanvas = null;
        while (burglarCanvas == null) {
            burglarCanvas = GameObject.FindObjectOfType<BurglarCanvasController>(true);
            yield return null;
        }

        inputProfile = InputProfile.allowAll with {
            allowPlayerFireInput = false
        };
        HighlightLocation("door");

        CutsceneManager.I.StartCoroutine(MoveCharacter(mentorController, "mentor_door", speedCoefficient: 0.75f));
        yield return WaitForTrigger("door");
        yield return WaitForFocus();
        GameObject.FindObjectOfType<BurglarCanvasController>(true).PreventClose(true);
        GameManager.I.uiController.ShowInteractiveHighlight();
        Time.timeScale = 1;
        inputProfile = InputProfile.allowNone with {
            allowCameraControl = true,
            allowPlayerFireInput = true
        };
        GameManager.I.uiController.ShowTutorialText("Mouse over the door and click on the <sprite name=\"screwdriver\"> icon to open the burglar view");
        HideLocationHighlight();
        yield return WaitForTrigger("burglar_open");
        inputProfile = InputProfile.allowNone;
        burglarCanvas.PreventButtons(true);
        burglarCanvas.PreventClose(true);

        yield return new WaitForSecondsRealtime(1f);
        yield return GameManager.I.uiController.ShowCutsceneDialogue("the mentor", mentorPortrait, "Okay kid, now we've gotta get through a locked door. What do you do in this situation?", location: CutsceneDialogueController.Location.top);
        RectTransform panelElement = GameObject.FindObjectsOfType<AttackSurfaceUIElement>()
            .Where(element => element.elementName == "outerLatch").Select(element => element.GetComponent<RectTransform>()).First();
        GameManager.I.uiController.DrawLine(panelElement, Vector3.zero, IndicatorUIController.Origin.bottom);
        yield return GameManager.I.uiController.ShowCutsceneDialogue("the mentor", mentorPortrait, "Most people see a locked door and reach for the lockpick, but that's a rookie mistake. The latch is a weak point, and easy to exploit if you know how.", location: CutsceneDialogueController.Location.top);
        GameManager.I.uiController.DrawLine(GameManager.I.uiController.indicatorUIController.probeIndicator, Vector3.zero, IndicatorUIController.Origin.bottom);
        yield return GameManager.I.uiController.ShowCutsceneDialogue("the mentor", mentorPortrait, "A well-placed shim can open most doors faster and quieter than any lockpick.", location: CutsceneDialogueController.Location.top);
        burglarCanvas.PreventButtons(false);
        inputProfile = InputProfile.allowAll;
        GameManager.I.uiController.ShowTutorialText("Use the probe tool on the latch");
        yield return WaitForTrigger("latch_bypass");
        inputProfile = InputProfile.allowNone;
        burglarCanvas.PreventButtons(true);
        yield return GameManager.I.uiController.ShowCutsceneDialogue("the mentor", mentorPortrait, "Nicely done. Remember, simpler is better in this line of work.");
        inputProfile = InputProfile.allowAll;
        yield return LeaveFocus();
        burglarCanvas.PreventButtons(false);
        burglarCanvas.PreventClose(false);
        GameManager.I.CloseBurglar();
    }

    IEnumerator KeyWatcher() {
        yield return WaitForTrigger("got_key");
        yield return WaitForFocus();
        yield return GameManager.I.uiController.ShowCutsceneDialogue("the mentor", mentorPortrait, "Good looking out. That key is exactly what we need.");
        yield return Toolbox.CoroutineFunc(() => hasKey = true);
        yield return LeaveFocus();
    }

    IEnumerator LootWatcher() {
        yield return WaitForTrigger("got_loot");
        yield return WaitForFocus();
        yield return GameManager.I.uiController.ShowCutsceneDialogue("the mentor", mentorPortrait, "Grabbing some loot, huh? Good idea. I know a guy back on shakedown who'll probably want to buy that from you.");
        yield return LeaveFocus();
    }

    IEnumerator Interior() {
        HideBasicUI();

        Coroutine waitForKey = CutsceneManager.I.StartCoroutine(KeyWatcher());

        Coroutine waitForLoot = CutsceneManager.I.StartCoroutine(LootWatcher());

        yield return MoveCharacter(mentorController, "mentor_interior");
        HighlightLocation("interior");
        yield return WaitForTrigger("interior");
        HideLocationHighlight();

        yield return WaitForFocus();
        yield return GameManager.I.uiController.ShowCutsceneDialogue("the mentor", mentorPortrait, "Okay. We know where the target is, so let's get moving.");
        yield return LeaveFocus();

        // yield return MoveCharacter(mentorController, "mentor_labdoor");
        CutsceneManager.I.StartCoroutine(MoveCharacter(mentorController, "mentor_labdoor", speedCoefficient: 0.75f));
        HighlightLocation("labdoor");
        yield return WaitForTrigger("labdoor");
        HideLocationHighlight();

        // yield return Toolbox.RunJobRepeatedly(() => WaitForKey())
        if (hasKey) {

        } else {
            yield return WaitForFocus();
            yield return GameManager.I.uiController.ShowCutsceneDialogue("the mentor", mentorPortrait, "This door is locked too. I think I saw the key back where we came in, go get it.");
            yield return LeaveFocus();
            GameManager.I.uiController.ShowTutorialText("Find the key and click on it to pick it up");

            // TODO: highlight key 
            yield return WaitForTrigger("got_key");
            HighlightLocation("labdoor");
            yield return WaitForTrigger("labdoor");
            HideLocationHighlight();
        }

        yield return WaitForFocus();
        Time.timeScale = 1f;
        // yield return GameManager.I.uiController.ShowCutsceneDialogue("the mentor", mentorPortrait, "Great. Let's try the key in the lab door here.");
        inputProfile = InputProfile.allowAll with {
            allowPlayerMovement = false,
            allowBurglarButton = false
        };
        GameManager.I.uiController.ShowUI();
        GameManager.I.uiController.ShowTutorialText("Click on the door");
        yield return WaitForTrigger("locked_door");
        GameManager.I.uiController.ShowStatusBar(true);
        GameManager.I.uiController.HideCutsceneDialogue();
        GameManager.I.uiController.DrawLine(GameManager.I.uiController.indicatorUIController.keyMenuIndicator, Vector3.zero);
        inputProfile = inputProfile with { allowCameraControl = false };
        yield return GameManager.I.uiController.ShowCutsceneDialogue("the mentor", mentorPortrait, "Since you just tried a locked door, the key menu opened up here. How do you know if the key works in the lock? You gotta try it out!");
        inputProfile = inputProfile with { allowCameraControl = true };
        // TODO: indicate key menu
        GameManager.I.uiController.ShowTutorialText("Click on the door to reveal the key menu\nClick on the key button to use the key in the lock");
        yield return WaitForTrigger("key_menu_used");
        GameManager.I.uiController.HideCutsceneDialogue();
        inputProfile = inputProfile with { allowCameraControl = false };
        yield return GameManager.I.uiController.ShowCutsceneDialogue("the mentor", mentorPortrait, "Using the right key in a locked door draws a lot less attention than trying to pick it. Makes it look like you belong there. Guards hassle you less.");
        yield return GameManager.I.uiController.ShowCutsceneDialogue("the mentor", mentorPortrait, "Even just knowing the key code can help you sometimes. Some runners like to cut their own keys or make their own keycards while on a mission.");
        inputProfile = inputProfile with { allowCameraControl = true };
        yield return LeaveFocus();

        CutsceneManager.I.StartCoroutine(MoveCharacter(mentorController, "mentor_lab", speedCoefficient: 0.75f));

        if (waitForKey != null) {
            CutsceneManager.I.StopCoroutine(waitForKey);
        }
        if (waitForLoot != null) {
            CutsceneManager.I.StopCoroutine(waitForLoot);
        }
    }

    IEnumerator Laboratory() {
        HighlightLocation("lab");
        yield return WaitForTrigger("lab");
        HideLocationHighlight();

        yield return WaitForFocus();
        GameManager.I.uiController.ShowOverlayControls(true);
        Time.timeScale = 1f;
        yield return GameManager.I.uiController.ShowCutsceneDialogue("the mentor", mentorPortrait, "Here we are. Get out your cyberdeck.");
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
        yield return GameManager.I.uiController.ShowCutsceneDialogue("the mentor", mentorPortrait, "Our objective is in that datastore but it's password protected. Let's crack the password.");

        GameManager.I.uiController.ShowTutorialText("Click on the datastore node (<sprite name=\"data_objective\">) to connect to it.");
        yield return WaitForTrigger("node_select_terminal");
        GameManager.I.uiController.HideCutsceneText();

        yield return new WaitForSecondsRealtime(1f);
        GameManager.I.uiController.ShowIndicator(GameManager.I.uiController.indicatorUIController.hackButtonIndicator, Vector3.zero, IndicatorUIController.Direction.right);
        GameManager.I.uiController.ShowTutorialText("Click the Hack... button to open the hack software interface");
        yield return WaitForTrigger("hack_software_open");
        GameManager.I.uiController.HideAllIndicators();
        GameManager.I.uiController.HideCutsceneText();

        GameManager.I.uiController.ShowIndicator(GameManager.I.uiController.softwareModalController.selectorIndicators["tumbler"], Vector3.left * 200f, IndicatorUIController.Direction.right);

        yield return WaitForTrigger("software_deploy_tumbler");
        GameManager.I.uiController.HideAllIndicators();

        yield return WaitForTrigger("software_complete_tumbler");
        inputProfile = inputProfile with {
            allowCameraControl = false
        };
        yield return new WaitForSecondsRealtime(1f);
        yield return GameManager.I.uiController.ShowCutsceneDialogue("the mentor", mentorPortrait, "The node's wide open. Let's grab that datafile and get out of here.");

        GameManager.I.uiController.ShowIndicator(GameManager.I.uiController.indicatorUIController.downloadButtonIndicator, Vector3.zero, IndicatorUIController.Direction.right);
        GameManager.I.uiController.ShowTutorialText("Click the Download... button");

    }

    void HideBasicUI() {
        GameManager.I.uiController.ShowStatusBar(false);
        GameManager.I.uiController.ShowAppearanceInfo(false);
        GameManager.I.uiController.ShowVisibilityInfo(false);
        GameManager.I.uiController.ShowOverlayControls(false);
    }
}