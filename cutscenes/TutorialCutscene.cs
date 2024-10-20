using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AI;
using Easings;
using Items;
using UnityEngine;

class TutorialCutscene : Cutscene {
    enum Phase { begin, network, stealth, loot }
    Phase phase;
    NPCTemplate mentorTemplate;
    Sprite mentorPortrait;
    GameObject vehicleObject;
    Light spotlight;
    NPCTemplate guardTemplate;
    Sprite guardPortrait;
    Sprite jackPortrait;
    GameObject rainParticles;
    AmbientZone[] rainAmbience;
    Door closetDoor;
    Coroutine gunPrevention;

    GameObject mentorObject;
    CharacterController mentorController;
    Vector2 zoomInput = new Vector2(0f, 5f);
    bool hasKey = false;


    public TutorialCutscene(NPCTemplate mentorTemplate,
                            Sprite mentorPortrait,
                            Sprite jackPortrait,
                            GameObject vehicleObject,
                            Light spotlight,
                            NPCTemplate guardTemplate,
                            Sprite guardPortrait,
                            GameObject rainParticles,
                            AmbientZone[] rainAmbience,
                            Door closetDoor) {
        this.mentorTemplate = mentorTemplate;
        this.mentorPortrait = mentorPortrait;
        this.vehicleObject = vehicleObject;
        this.spotlight = spotlight;
        this.guardTemplate = guardTemplate;
        this.guardPortrait = guardPortrait;
        this.rainParticles = rainParticles;
        this.rainAmbience = rainAmbience;
        this.jackPortrait = jackPortrait;
        this.closetDoor = closetDoor;
    }

    public override void Cleanup() {
        UIController.onUIChange -= HideBasicUI;
        GameManager.I.doPlayerInput = true;
        if (gunPrevention != null) {
            CutsceneManager.I.StopCoroutine(gunPrevention);
        }
    }

    public override IEnumerator DoCutscene() {
        // HideBasicUI();
        UIController.onUIChange += HideBasicUI;

        mentorObject = CutsceneManager.I.SpawnNPC("mentorSpawn", mentorTemplate);
        mentorController = mentorObject.GetComponent<CharacterController>();
        SphereRobotAI mentorAI = mentorObject.GetComponent<SphereRobotAI>();
        mentorAI.enabled = false;
        spotlight.enabled = false;

        // ScriptSceneLocation mentorLockpickData = CutsceneManager.I.worldLocations["mentor_greet"];
        // mentorObject.transform.position = mentorLockpickData.transform.position;
        // mentorController.Motor.SetPosition(mentorLockpickData.transform.position, bypassInterpolation: true);
        // mentorObject.SetActive(true);

        // ScriptSceneLocation mentorLockpickData = CutsceneManager.I.worldLocations["mentor_power"];
        // mentorObject.transform.position = mentorLockpickData.transform.position;
        // mentorController.Motor.SetPosition(mentorLockpickData.transform.position, bypassInterpolation: true);
        // mentorObject.SetActive(true);

        // mentor_interior
        // ScriptSceneLocation mentorLockpickData = CutsceneManager.I.worldLocations["mentor_interior"];
        // mentorObject.transform.position = mentorLockpickData.transform.position;
        // mentorController.Motor.SetPosition(mentorLockpickData.transform.position, bypassInterpolation: true);
        // mentorObject.SetActive(true);


        // ScriptSceneLocation mentorLockpickData = CutsceneManager.I.worldLocations["mentor_lab"];
        // mentorObject.transform.position = mentorLockpickData.transform.position;
        // mentorController.Motor.SetPosition(mentorLockpickData.transform.position, bypassInterpolation: true);
        // mentorObject.SetActive(true);

        yield return FromStartToGreet();

        yield return GreetHackSequence();

        yield return Fence();

        yield return PowerHack();

        yield return EnterDoor();

        // yield return new WaitForSecondsRealtime(3);

        yield return Interior();

        yield return Laboratory();

        // yield return new WaitForSecondsRealtime(3);
        yield return MyWaitForFocus();

        yield return Conclusion();

        yield return Escape();

        GameManager.I.StartNewGameBarSequence();
    }

    IEnumerator FromStartToGreet() {

        yield return MyWaitForFocus();
        GameManager.I.uiController.keyMenuButtonGroup.interactable = false;

        Time.timeScale = 1f;
        SetCameraPosition("alleystart", CameraState.normal, orthographicSize: 5f, snapToOrthographicSize: true, snapToPosition: true);

        yield return Toolbox.Parallelize(MoveCharacter(mentorController, "mentorWalk1"), MoveCharacter(GameManager.I.playerCharacterController, "playerWalk1"));
        yield return ShowCutsceneDialogue("the mentor", mentorPortrait, "Alright, kid, listen up. This ain't no heist. It's a stroll in the park. We're in, we're out, no alarms, no drama. Got it?");
        yield return ShowCutsceneDialogue("the mentor", mentorPortrait, "Our target is in a data warehouse just down this alley. I'll go ahead to scout it out, you follow behind me. Remember, eyes and ears open.");
        yield return MoveCharacter(mentorController, "mentorWalk2");

        // move mentor to mentor_greet
        ScriptSceneLocation mentorLockpickData = CutsceneManager.I.worldLocations["mentor_greet"];
        mentorObject.transform.position = mentorLockpickData.transform.position;
        mentorController.Motor.SetPosition(mentorLockpickData.transform.position, bypassInterpolation: true);
        mentorObject.SetActive(true);

        yield return MyLeaveFocus();
        GameManager.I.uiController.ShowTutorialText("Movement: WASD");
        yield return WaitForTrigger("walk1");
        GameManager.I.uiController.ShowTutorialText("Rotate camera: Q / E\nZoom camera: mouse wheel");
        yield return WaitForTrigger("crawl");
        yield return MyWaitForFocus();
        playerCharacterController.TransitionToState(CharacterState.normal);

        yield return RotateIsometricCamera(IsometricOrientation.NW, playerCharacterController.transform.position);
        ScriptSceneCameraPosition crawlData = CutsceneManager.I.cameraLocations["crawl"];
        GameManager.I.uiController.ShowTutorialText("Hold Control while moving to crawl");
        yield return MoveCamera(crawlData.transform.position, crawlData.transform.rotation, 2f, CameraState.normal, zoomInput, PennerDoubleAnimation.ExpoEaseOut);
        yield return new WaitForSecondsRealtime(1f);
        yield return MyLeaveFocus();


        yield return WaitForTrigger("ladder");
        yield return MyWaitForFocus();
        yield return RotateIsometricCamera(IsometricOrientation.NW, playerCharacterController.transform.position);

        yield return MoveCamera("ladder", 1.5f, CameraState.normal, Vector2.zero, PennerDoubleAnimation.ExpoEaseOut);

        if (playerCharacterController.gunHandler.HasGun()) {
            GameManager.I.uiController.ShowTutorialText("Right click the ladder to climb");
        } else {
            GameManager.I.uiController.ShowTutorialText("Click the ladder to climb");
        }
        yield return new WaitForSecondsRealtime(1.7f);
        yield return MyLeaveFocus();
        HighlightLocation("ladder");
        yield return WaitForTrigger("interact_ladder");
        HideLocationHighlight();
        GameManager.I.uiController.ShowTutorialText("Use W / S to climb up and down the ladder\nClimb to the top to dismount");
        yield return WaitForTrigger("ladder_dismount");
        GameManager.I.uiController.HideCutsceneText();
        inputProfile = inputProfile with {
            allowPlayerFireInput = false
        };
    }

    IEnumerator GreetHackSequence() {
        /* greet sequence */
        phase = Phase.network;
        // yield return WaitForTrigger("greet");
        yield return MyWaitForFocus();
        playerCharacterController.TransitionToState(CharacterState.normal);
        HideBasicUI();

        Time.timeScale = 1f;
        yield return MoveCharacter(playerCharacterController, "player_greet", speedCoefficient: 1f);
        CharacterLookAt(playerCharacterController, "building_look");
        CharacterLookAt(mentorController, "building_look");
        yield return MoveCamera("greet", 0.5f, CameraState.free);
        yield return new WaitForSecondsRealtime(1f);
        yield return ShowCutsceneDialogue("the mentor", mentorPortrait, "Alright kid, this is the place. Quiet night, all the better for us.");
        yield return ShowCutsceneDialogue("the mentor", mentorPortrait, "Before we go poking around blind, let's scan the network and find out exactly where the target datafile is hiding.");
        yield return new WaitForSecondsRealtime(0.25f);
        yield return MyLeaveFocus();

        if (playerCharacterController.gunHandler.HasGun()) {
            yield return new WaitForSecondsRealtime(0.5f);

            InputProfile origInput = inputProfile;
            yield return MyWaitForFocus();
            inputProfile = InputProfile.allowNone;
            yield return new WaitForSecondsRealtime(0.5f);
            yield return ShowCutsceneDialogue("the mentor", mentorPortrait, "By the way, put that gun away. We're thieves, not murderers. We're keeping a low profile, remember?");
            yield return new WaitForSecondsRealtime(0.5f);
            playerCharacterController.HandleSwitchWeapon(-1);
            inputProfile = origInput;
            yield return MyLeaveFocus();
        }

        gunPrevention = CutsceneManager.I.StartCoroutine(Toolbox.RunJobRepeatedly(() => GunPrevention()));


        Coroutine mentorWalking = CutsceneManager.I.StartCoroutine(MoveCharacter(mentorController, "mentor_kiosk", speedCoefficient: 1f));
        HighlightLocation("player_kiosk");

        yield return WaitForTrigger("kiosk");
        HideLocationHighlight();
        yield return MyWaitForFocus();
        playerCharacterController.TransitionToState(CharacterState.normal);
        Time.timeScale = 1f;
        CharacterLookAt(mentorController, "player_kiosk");
        GameManager.I.uiController.ShowOverlayControls(true);
        yield return ShowCutsceneDialogue("the mentor", mentorPortrait, "We should be able to tap into their network from this wallcom. Get out your cyberdeck.");
        GameManager.I.uiController.overlayHandler.overrideDisconnectButton = true;

        inputProfile = inputProfile with {
            allowePlayerItemSelect = true,
            allowCameraControl = true
        };

        if (GameManager.I.playerItemHandler.activeItem != null && GameManager.I.playerItemHandler.activeItem is CyberDeck) {

        } else {
            GameManager.I.uiController.ShowTutorialText("Hold X to open inventory menu. Select cyberdeck and release X.");
            yield return WaitForTrigger("item_select_deck");
        }

        inputProfile = InputProfile.allowNone with {
            allowCameraControl = true
        };

        GameManager.I.uiController.HideCutsceneDialogue();
        yield return RotateIsometricCamera(IsometricOrientation.NW, playerCharacterController.transform.position);
        yield return CameraIsometricZoom(2f, 0.5f);

        GameManager.I.uiController.ShowTutorialText("Click the wallcom node <sprite name=\"polygon\"> to connect to it.");
        yield return WaitForTrigger("node_select_wallcom");
        GameManager.I.uiController.HideCutsceneText();
        GameManager.I.uiController.overlayGroup.interactable = false;

        yield return new WaitForSecondsRealtime(1.5f);
        yield return RotateIsometricCamera(IsometricOrientation.SW, playerCharacterController.transform.position);
        yield return CameraIsometricZoom(5f, 0.5f);

        yield return new WaitForSecondsRealtime(1.5f);
        yield return ShowCutsceneDialogue("the mentor", mentorPortrait, "Okay, we're looking at the network now. ");

        yield return new WaitForSecondsRealtime(1.2f);
        GameManager.I.uiController.DrawLine(GameManager.I.uiController.overlayHandler.cyberOverlay.GetIndicator("cyberdeck").rectTransform, Vector3.zero);
        yield return ShowCutsceneDialogue("the mentor", mentorPortrait, "This node here is your cyberdeck and the node it is connected to is the wallcom.");

        GameManager.I.uiController.DrawLine(GameManager.I.uiController.indicatorUIController.cyberInfoIndicatorAnchor, Vector3.zero);
        yield return ShowCutsceneDialogue("the mentor", mentorPortrait, "This display shows info about your current target."); // TODO

        GameManager.I.uiController.DrawLine(GameManager.I.uiController.indicatorUIController.hackIndicatorAnchor, Vector3.zero);
        yield return ShowCutsceneDialogue("the mentor", mentorPortrait, "Your cyberdeck is how you launch attacks against the target."); // TODO
        GameManager.I.uiController.HideAllIndicators();

        inputProfile = inputProfile with {
            allowCameraControl = false
        };

        // TODO: zoom out
        yield return MoveCamera("router", 1f, CameraState.normal);
        CharacterLookAt(mentorController, "router");
        yield return ShowCutsceneDialogue("the mentor", mentorPortrait, "We want to get up to that router to see if it can let us in the building.");
        inputProfile = inputProfile with {
            allowCameraControl = true
        };
        GameManager.I.uiController.overlayGroup.interactable = true;

        GameManager.I.uiController.ShowTutorialText("Click the router node <sprite name=\"polygon\"> to connect to it.\nQ/E and scroll wheel to control camera");
        GameManager.I.uiController.ShowIndicator(GameManager.I.uiController.overlayHandler.cyberOverlay.GetIndicator("router").rectTransform, Vector3.left * 25f, IndicatorUIController.Direction.right);

        yield return WaitForTrigger("node_select_router");

        GameManager.I.uiController.SetOverlayButtonInteractible(false);
        GameManager.I.uiController.overlayGroup.interactable = false;

        GameManager.I.uiController.HideAllIndicators();
        GameManager.I.uiController.HideCutsceneText();
        yield return new WaitForSecondsRealtime(1f);
        yield return ShowCutsceneDialogue("the mentor", mentorPortrait, "Alright, this node is probably connected to the target building. We need to scan it to reveal its connections.");

        GameManager.I.uiController.ShowIndicator(GameManager.I.uiController.overlayHandler.cyberOverlay.hackTerminalController.selectorIndicators["scan"], Vector3.left * 20f, IndicatorUIController.Direction.right);
        GameManager.I.uiController.ShowTutorialText("Select the scan software");
        GameManager.I.uiController.overlayGroup.interactable = true;

        yield return WaitForTrigger("software_view_scan");
        GameManager.I.uiController.HideAllIndicators();
        GameManager.I.uiController.ShowTutorialText("Deploy the scan software");
        GameManager.I.uiController.ShowIndicator(GameManager.I.uiController.indicatorUIController.deployButtonIndicator, Vector3.zero, IndicatorUIController.Direction.right);
        yield return WaitForTrigger("software_deploy_scan");
        GameManager.I.uiController.HideCutsceneText();
        GameManager.I.uiController.HideAllIndicators();
        yield return WaitForTrigger("software_complete_scan");
        GameManager.I.uiController.overlayGroup.interactable = false;

        inputProfile = inputProfile with {
            allowCameraControl = false
        };
        yield return RotateIsometricCamera(IsometricOrientation.NW, playerCharacterController.transform.position);
        yield return new WaitForSecondsRealtime(0.5f);
        yield return ShowCutsceneDialogue("the mentor", mentorPortrait, "Bingo. There's our way in.");
        yield return new WaitForSecondsRealtime(0.5f);

        inputProfile = inputProfile with {
            allowCameraControl = true
        };
        GameManager.I.uiController.overlayGroup.interactable = true;

        GameManager.I.uiController.ShowTutorialText("Click the unknown router node <sprite name=\"polygon\"> to connect to it.\nQ/E and scroll wheel to control camera");
        GameManager.I.uiController.ShowIndicator(GameManager.I.uiController.overlayHandler.cyberOverlay.GetIndicator("router 2").rectTransform, Vector3.left * 25f, IndicatorUIController.Direction.right);
        yield return WaitForTrigger("node_select_router 2");
        GameManager.I.uiController.overlayGroup.interactable = false;

        GameManager.I.uiController.HideAllIndicators();
        GameManager.I.uiController.HideCutsceneText();

        yield return ShowCutsceneDialogue("the mentor", mentorPortrait, "Looks like the lab, our target data file is somewhere in there. We're too far away from your cyberdeck to scan more from here though.");
        yield return ShowCutsceneDialogue("the mentor", mentorPortrait, "We'll need to take control of that router and use it to launch our attacks.");

        GameManager.I.uiController.ShowIndicator(GameManager.I.uiController.overlayHandler.cyberOverlay.GetIndicator("router").rectTransform, Vector3.left * 25f, IndicatorUIController.Direction.right);
        GameManager.I.uiController.ShowTutorialText("Click the router node <sprite name=\"polygon\"> to connect to it.\nQ/E and scroll wheel to control camera");
        GameManager.I.uiController.overlayGroup.interactable = true;

        yield return WaitForTrigger("node_select_router");
        GameManager.I.uiController.HideAllIndicators();

        yield return new WaitForSecondsRealtime(1f);
        GameManager.I.uiController.ShowIndicator(GameManager.I.uiController.overlayHandler.cyberOverlay.hackTerminalController.selectorIndicators["exploit"], Vector3.left * 20f, IndicatorUIController.Direction.right);
        GameManager.I.uiController.ShowTutorialText("Select the exploit software");
        yield return WaitForTrigger("software_view_exploit");
        GameManager.I.uiController.HideAllIndicators();
        GameManager.I.uiController.ShowTutorialText("Deploy the exploit software");
        GameManager.I.uiController.ShowIndicator(GameManager.I.uiController.indicatorUIController.deployButtonIndicator, Vector3.zero, IndicatorUIController.Direction.right);
        yield return WaitForTrigger("software_deploy_exploit");
        GameManager.I.uiController.HideCutsceneText();
        GameManager.I.uiController.HideAllIndicators();
        yield return WaitForTrigger("software_complete_exploit");

        GameManager.I.uiController.ShowIndicator(GameManager.I.uiController.overlayHandler.cyberOverlay.GetIndicator("router 2").rectTransform, Vector3.left * 25f, IndicatorUIController.Direction.right);
        GameManager.I.uiController.ShowTutorialText("Click the router node <sprite name=\"polygon\"> to connect to it.");
        yield return WaitForTrigger("node_select_router 2");

        GameManager.I.uiController.ShowIndicator(GameManager.I.uiController.overlayHandler.cyberOverlay.hackTerminalController.selectorIndicators["scan"], Vector3.left * 20f, IndicatorUIController.Direction.right);
        yield return WaitForTrigger("software_view_scan");
        GameManager.I.uiController.HideAllIndicators();
        GameManager.I.uiController.ShowTutorialText("Deploy the scan software");
        GameManager.I.uiController.ShowIndicator(GameManager.I.uiController.indicatorUIController.deployButtonIndicator, Vector3.zero, IndicatorUIController.Direction.right);
        yield return WaitForTrigger("software_deploy_scan");
        GameManager.I.uiController.HideCutsceneText();
        GameManager.I.uiController.HideAllIndicators();
        yield return WaitForTrigger("software_complete_scan");

        GameManager.I.uiController.overlayGroup.interactable = false;

        yield return new WaitForSecondsRealtime(0.5f);
        yield return ShowCutsceneDialogue("the mentor", mentorPortrait, "Just as I thought. There's three cyber nodes in there and one of them should have our data.");
        yield return ShowCutsceneDialogue("the mentor", mentorPortrait, "Use the data scan on each node to find which one has our target.");

        GameManager.I.uiController.overlayGroup.interactable = true;
        GameManager.I.uiController.ShowTutorialText("Use the data scan software on each node until you find the target <sprite name=\"data_objective\">.");

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
        GameManager.I.uiController.overlayGroup.interactable = false;

        yield return ShowCutsceneDialogue("the mentor", mentorPortrait, "Great work. There's our package.");
        yield return ShowCutsceneDialogue("the mentor", mentorPortrait, "Alright, we've got our target. You can keep snooping the net if you want. When you're ready to roll, hit the disconnect and meet me by the fence.");
        GameManager.I.uiController.overlayGroup.interactable = true;
        GameManager.I.uiController.overlayHandler.overrideDisconnectButton = false;
        GameManager.I.uiController.overlayHandler.closeButtonObject.SetActive(true);
        GameManager.I.uiController.SetOverlayButtonInteractible(true);

        // click "DISCONNECT" button
        yield return MyLeaveFocus();
        CutsceneManager.I.StartCoroutine(MoveCharacter(mentorController, "mentor_lockpick", speedCoefficient: 0.4f));

        GameManager.I.uiController.ShowOverlayControls(true);

        GameManager.I.uiController.ShowIndicator(GameManager.I.uiController.indicatorUIController.disconnectIndicator, Vector3.zero, IndicatorUIController.Direction.up);
        GameManager.I.uiController.ShowTutorialText("Click the disconnect button");
        yield return WaitForTrigger("disconnect");
        playerCharacterController.itemHandler.ClearItem();
        GameManager.I.uiController.HideAllIndicators();
        GameManager.I.uiController.HideCutsceneText();
    }

    IEnumerator Fence() {
        phase = Phase.stealth;

        HighlightLocation("lockpick");
        yield return WaitForTrigger("lockpick2");
        yield return MyWaitForFocus();
        playerCharacterController.TransitionToState(CharacterState.normal);
        Time.timeScale = 1;
        yield return RotateIsometricCamera(IsometricOrientation.NE, playerCharacterController.transform.position);
        HideLocationHighlight();
        yield return MoveCharacter(playerCharacterController, "lockpick", 0.5f);
        yield return CameraIsometricZoom(2f, 0.5f);

        yield return ShowCutsceneDialogue("the mentor", mentorPortrait, "Alright. Did you remember to bring the fence cutters? Let's get them out.");
        inputProfile = InputProfile.allowNone with {
            allowePlayerItemSelect = true,
            allowCameraControl = true
        };
        GameManager.I.uiController.ShowTutorialText("Hold X to open inventory menu. Select fence cutters and release X.");

        yield return WaitForTrigger("item_select_fence cutters");
        inputProfile = InputProfile.allowNone;
        GameManager.I.uiController.HideCutsceneDialogue();
        yield return ShowCutsceneDialogue("the mentor", mentorPortrait, "Perfect. Now let's make ourselves a door.");
        // TODO: health bar
        inputProfile = InputProfile.allowNone with {
            allowCameraControl = true,
            allowPlayerFireInput = true
        };
        GameManager.I.uiController.ShowTutorialText("With the fence cutters selected, click the fence to cut.");
        yield return WaitForTrigger("fence cut fence_wall");
        inputProfile = InputProfile.allowNone;
        GameManager.I.doPlayerInput = false;

        GameManager.I.uiController.HideCutsceneDialogue();
        yield return new WaitForSecondsRealtime(1f);

        GameManager.I.uiController.ShowAppearanceInfo(true);
        yield return new WaitForSecondsRealtime(1f);

        GameManager.I.uiController.DrawLine(GameManager.I.uiController.indicatorUIController.suspicionMeterIndicator, Vector3.zero);
        yield return ShowCutsceneDialogue("the mentor", mentorPortrait, "Before we go in there, check the suspicion meter. It shows your appearance and the guards' response. We're in a restricted area and if we are spotted they will want some answers.");
        yield return ShowCutsceneDialogue("the mentor", mentorPortrait, "If guards find evidence like this broken fence, they'll know someone has been here. They'll be more suspicious or aggressive, so keep a low profile.");

        GameManager.I.uiController.HideAllIndicators();

        yield return ShowCutsceneDialogue("the mentor", mentorPortrait, "Alright, let's go. Follow me.");
        yield return MoveCharacter(mentorController, "crawlpoint1", crawling: true);
        yield return MoveCharacter(mentorController, "hide1", speedCoefficient: 0.6f);
        yield return MoveCamera("hide", 1f, CameraState.normal);

        SelectItem(playerCharacterController);
        yield return MoveCharacter(playerCharacterController, "crawlpoint1", crawling: true);
        yield return MoveCharacter(playerCharacterController, "hide2", speedCoefficient: 0.6f);
        GameManager.I.doPlayerInput = true;

        yield return new WaitForSecondsRealtime(0.75f);
        yield return ShowCutsceneDialogue("the mentor", mentorPortrait, "Okay, careful now. See that camera over there?");
        CharacterLookAt(mentorController, "camera2");
        CharacterLookAt(playerCharacterController, "camera2");

        yield return MoveCamera("camera", 1.5f, CameraState.normal, Vector2.zero, PennerDoubleAnimation.ExpoEaseOut);
        yield return CameraIsometricZoom(4.5f, 0.5f);
        yield return new WaitForSecondsRealtime(0.75f);

        yield return ShowCutsceneDialogue("the mentor", mentorPortrait, "It's pointing right at the door we want. If we move in now, we'll be spotted for sure.");
        yield return ShowCutsceneDialogue("the mentor", mentorPortrait, "Let's scope out the situation before we make a move. Switch to the alarm network display.");

        GameManager.I.uiController.ShowOverlayControls(true);
        GameManager.I.uiController.ShowOverlayButtonHighlight(true);
        yield return WaitForTrigger("overlay_change_3");
        GameManager.I.uiController.ShowOverlayButtonHighlight(false);
        yield return new WaitForSecondsRealtime(0.25f);
        GameManager.I.uiController.SetOverlayButtonInteractible(false);
        yield return MoveCamera("camera", 0.25f, CameraState.normal, Vector2.zero, PennerDoubleAnimation.ExpoEaseOut);

        yield return ShowCutsceneDialogue("the mentor", mentorPortrait, "Just as I thought. It's wired right in to the central alarm system. If it spots us, security will be on us quick.");
        yield return ShowCutsceneDialogue("the mentor", mentorPortrait, "There might be an easy way to take it out. Change your overlay to the power network.");
        GameManager.I.uiController.SetOverlayButtonInteractible(true);
        GameManager.I.uiController.ShowOverlayButtonHighlight(true);
        yield return WaitForTrigger("overlay_change_1");
        GameManager.I.uiController.ShowOverlayButtonHighlight(false);
        GameManager.I.uiController.SetOverlayButtonInteractible(false);

        yield return new WaitForSecondsRealtime(0.75f);
        yield return MoveCamera("powerbox", 1.5f, CameraState.normal, Vector2.zero, PennerDoubleAnimation.ExpoEaseOut);
        yield return ShowCutsceneDialogue("the mentor", mentorPortrait, "If we cut the power from that relay, the camera will be out of the picture. Let's go.");

        yield return MyLeaveFocus();
        GameManager.I.uiController.HideInteractiveHighlight();
        GameManager.I.SetOverlay(OverlayType.none);
        // yield return MoveCamera("lockpick", 1.5f, CameraState.normal, zoomInput, PennerDoubleAnimation.ExpoEaseOut);
        // yield return ShowCutsceneDialogue("the mentor", mentorPortrait, "Hey Jack, get over here! I need you to open this lock.");
        // yield return CutsceneManager.I.LeaveFocus(this);
        // HideBasicUI();

        // yield return WaitForTrigger("lockpick2");
        // GameManager.I.uiController.ShowTutorialText("Mouse over the door and click the <sprite name=\"screwdriver\"> icon to open the burglar view");

        // yield return WaitForTrigger("burglar_open");
        // GameManager.I.uiController.tutorialBurglarInterrupt = true;
        // yield return new WaitForSecondsRealtime(0.75f);
        // yield return ShowCutsceneDialogue("the mentor", mentorPortrait, "Okay, this is the burglar interface. Let's get your lockpick out.");
        // GameManager.I.uiController.tutorialBurglarInterrupt = false;
        // GameManager.I.uiController.tutorialBurglarAwaitLockpick = true;


        // yield return WaitForTrigger("burglartool_lockpick");
        // yield return new WaitForSecondsRealtime(0.1f);
        // GameManager.I.uiController.tutorialBurglarInterrupt = true;
        // yield return ShowCutsceneDialogue("the mentor", mentorPortrait, "Great. Now use it on the lock. Hold the mouse button until lockpicking is finished.");
        // GameManager.I.uiController.tutorialBurglarInterrupt = false;
        // GameManager.I.uiController.tutorialBurglarAwaitLockpick = false;

        // // await lock picked
        // yield return WaitForTrigger("lock_picked");
        // yield return new WaitForSecondsRealtime(1.5f);
        // GameManager.I.CloseBurglar();
        // yield return new WaitForSecondsRealtime(0.1f);
        // yield return MyWaitForFocus();
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
        yield return MyWaitForFocus();
        playerCharacterController.TransitionToState(CharacterState.normal);
        burglarCanvas.PreventClose(true);
        GameManager.I.uiController.ShowInteractiveHighlight();
        Time.timeScale = 1;
        inputProfile = InputProfile.allowNone with {
            allowCameraControl = true,
            allowPlayerFireInput = true,
            allowBurglarButton = true
        };
        GameManager.I.uiController.ShowTutorialText("Mouse over the power relay and click the <sprite name=\"screwdriver\"> icon to open the burglar view");
        HideLocationHighlight();
        yield return WaitForTrigger("burglar_open");
        burglarCanvas.PreventButtons(true);
        inputProfile = InputProfile.allowNone;
        yield return new WaitForSecondsRealtime(1f);
        yield return ShowCutsceneDialogue("the mentor", mentorPortrait, "Alright kid, this is your basic burglar toolkit.", location: CutsceneDialogueController.Location.top);

        GameManager.I.uiController.DrawLine(GameManager.I.uiController.indicatorUIController.probeIndicator, Vector3.zero, IndicatorUIController.Origin.bottom);

        yield return ShowCutsceneDialogue("the mentor", mentorPortrait, "This is a probe / decoder. It can bypass door latches and decode combination padlocks.", location: CutsceneDialogueController.Location.top);

        GameManager.I.uiController.DrawLine(GameManager.I.uiController.indicatorUIController.lockpickIndicator, Vector3.zero, IndicatorUIController.Origin.bottom);

        yield return ShowCutsceneDialogue("the mentor", mentorPortrait, "This the lockpick. It can unlock tumbler locks that use a physical key.", location: CutsceneDialogueController.Location.top);

        GameManager.I.uiController.DrawLine(GameManager.I.uiController.indicatorUIController.wirecuttersIndicator, Vector3.zero, IndicatorUIController.Origin.bottom);

        yield return ShowCutsceneDialogue("the mentor", mentorPortrait, "The wirecutters are self-explanatory. I wager we'll need them in a minute.", location: CutsceneDialogueController.Location.top);

        GameManager.I.uiController.DrawLine(GameManager.I.uiController.indicatorUIController.screwdriverIndicator, Vector3.zero, IndicatorUIController.Origin.bottom);

        yield return ShowCutsceneDialogue("the mentor", mentorPortrait, "Now use this screwdriver to remove the screws on this panel.", location: CutsceneDialogueController.Location.top);
        GameManager.I.uiController.ShowTutorialText("Click the screwdriver to select it, then click the screws to remove them");


        inputProfile.allowBurglarInterface = true;
        burglarCanvas.PreventButtons(false);

        inputProfile = InputProfile.allowAll;

        yield return WaitForTrigger("unscrew");
        yield return WaitForTrigger("unscrew");
        GameManager.I.uiController.HideCutsceneText();
        burglarCanvas.PreventButtons(false);

        GameManager.I.uiController.ShowTutorialText("Press escape or click the hand button <sprite name=\"hand\"> to put the screwdriver away.");
        yield return WaitForTrigger("burglartool_none");
        GameManager.I.uiController.HideCutsceneText();

        inputProfile.allowBurglarInterface = false;
        burglarCanvas.PreventButtons(true);

        yield return new WaitForSecondsRealtime(0.5f);

        RectTransform panelElement = GameObject.FindObjectsOfType<AttackSurfaceUIElement>()
                    .Where(element => element.elementName == "panel").Select(element => element.GetComponent<RectTransform>()).First();
        GameManager.I.uiController.DrawLine(panelElement, Vector3.zero, IndicatorUIController.Origin.top);
        yield return ShowCutsceneDialogue("the mentor", mentorPortrait, "Now let's pop that panel open and see what we can see.");
        inputProfile.allowBurglarInterface = true;
        burglarCanvas.PreventButtons(false);


        GameManager.I.uiController.ShowTutorialText("With hand tool <sprite name=\"hand\"> selected, click the panel to open it");
        yield return WaitForTrigger("panel_open");
        GameManager.I.uiController.HideCutsceneText();

        GameManager.I.uiController.ShowTutorialText("With hand tool <sprite name=\"hand\"> selected, click the circuit panel to inspect");
        yield return WaitForTrigger("circuit_view");
        GameManager.I.uiController.HideCutsceneText();
        GameObject.FindObjectOfType<BurglarCanvasController>(true).ShowReturnButton(false);

        inputProfile.allowBurglarInterface = false;
        burglarCanvas.PreventButtons(true);


        yield return new WaitForSecondsRealtime(1f);

        yield return ShowCutsceneDialogue("the mentor", mentorPortrait, "Okay kid, at this point it's a sitting duck. See those green wires? One of them's connected to the camera and the other goes to the main power source.");
        yield return ShowCutsceneDialogue("the mentor", mentorPortrait, "All you have to do is cut one of the wires, that will be enough to bring the camera down.");
        inputProfile.allowBurglarInterface = true;
        burglarCanvas.PreventButtons(false);

        GameManager.I.uiController.ShowTutorialText("Use the wirecutters to cut one of the green wires");


        yield return WaitForTrigger("wire_cut");
        if (wanderPrevention != null) CutsceneManager.I.StopCoroutine(wanderPrevention);

        inputProfile.allowBurglarInterface = false;
        burglarCanvas.PreventButtons(true);

        yield return new WaitForSecondsRealtime(0.5f);
        yield return ShowCutsceneDialogue("the mentor", mentorPortrait, "Perfect. You've got a knack for this. Let's clean up and get out of here.");
        inputProfile.allowBurglarInterface = true;
        burglarCanvas.PreventButtons(false);


        GameManager.I.uiController.ShowTutorialText("Press escape to leave the burglar view");
        burglarCanvas.PreventClose(false);

        yield return WaitForTrigger("burglar_close");
        GameManager.I.uiController.HideInteractiveHighlight();
        HideBasicUI();
        inputProfile = InputProfile.allowNone;
        yield return new WaitForSecondsRealtime(0.25f);
        yield return CameraIsometricZoom(4f, 0.5f);
        yield return MoveCamera("camera2", 1.5f, CameraState.normal, Vector2.zero, PennerDoubleAnimation.ExpoEaseOut);
        yield return ShowCutsceneDialogue("the mentor", mentorPortrait, "Excellent work. The camera's offline. Now, let's move quickly. The longer we're in here, the higher the risk of someone noticing. Let's grab the data and get out.");
        yield return MyLeaveFocus();

        GameManager.I.SetOverlay(OverlayType.none);
    }
    IEnumerator WanderPrevention() {
        yield return WaitForTrigger("wander");
        yield return MyWaitForFocus();
        playerCharacterController.TransitionToState(CharacterState.normal);
        Time.timeScale = 1f;
        yield return ShowCutsceneDialogue("the mentor", mentorPortrait, "Hey! What did I tell you? Get back over here!");
        yield return MoveCharacter(playerCharacterController, "wanderback", speedCoefficient: 1.2f);
        yield return ShowCutsceneDialogue("the mentor", mentorPortrait, "You go wandering off like that and it could blow the whole mission! Now pay attention!");
        yield return MyLeaveFocus();
        GameManager.I.uiController.HideInteractiveHighlight();
    }

    IEnumerator LabWanderPrevention() {
        yield return WaitForTrigger("wander2");
        yield return MyWaitForFocus();
        playerCharacterController.TransitionToState(CharacterState.normal);
        Time.timeScale = 1f;
        yield return ShowCutsceneDialogue("the mentor", mentorPortrait, "Hey! Don't go wandering off!");
        yield return MoveCharacter(playerCharacterController, "wanderback2", speedCoefficient: 1.2f);
        yield return ShowCutsceneDialogue("the mentor", mentorPortrait, "We've opened the pocket, now it's time to lift the wallet. I need you to focus.");
        yield return MyLeaveFocus();
    }

    IEnumerator GunPrevention() {
        yield return WaitForTrigger("gun_drawn");
        float origTimescale = Time.timeScale;
        InputProfile origInput = inputProfile with { };
        bool origHasFocus = hasFocus;
        bool interactiveHighlightenabled = GameManager.I.uiController.interactiveHighlightCanvas.enabled;
        // GameManager.I.uiController.ShowInteractiveHighlight();
        // GameManager.I.uiController.ShowInteractiveHighlight();
        if (!origHasFocus) {
            yield return MyWaitForFocus();
            playerCharacterController.TransitionToState(CharacterState.normal);
        }
        Time.timeScale = 1f;
        inputProfile = InputProfile.allowNone;
        yield return new WaitForSecondsRealtime(0.5f);
        yield return ShowCutsceneDialogue("the mentor", mentorPortrait, "Hey! Put the piece away! You're attracting attention! Do you want to get us killed?");
        yield return new WaitForSecondsRealtime(0.5f);
        playerCharacterController.HandleSwitchWeapon(-1);

        if (!origHasFocus) {
            yield return MyLeaveFocus();
            GameManager.I.uiController.interactiveHighlightCanvas.enabled = interactiveHighlightenabled;
        }
        Time.timeScale = origTimescale;
        inputProfile = origInput;
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
        GameManager.I.uiController.HideInteractiveHighlight();

        yield return WaitForTrigger("door");
        yield return MyWaitForFocus();
        playerCharacterController.TransitionToState(CharacterState.normal);
        GameManager.I.uiController.ShowInteractiveHighlight();
        GameObject.FindObjectOfType<BurglarCanvasController>(true).PreventClose(true);
        Time.timeScale = 1;
        inputProfile = InputProfile.allowNone with {
            allowCameraControl = true,
            allowPlayerFireInput = true,
            allowBurglarButton = true
        };
        GameManager.I.uiController.ShowTutorialText("Mouse over the door and click the <sprite name=\"screwdriver\"> icon to open the burglar view");
        HideLocationHighlight();
        yield return WaitForTrigger("burglar_open");
        inputProfile = InputProfile.allowNone;
        burglarCanvas.PreventButtons(true);
        burglarCanvas.PreventClose(true);

        yield return new WaitForSecondsRealtime(1f);
        yield return ShowCutsceneDialogue("the mentor", mentorPortrait, "Okay kid, now we've gotta get through a locked door. What do you do in this situation?", location: CutsceneDialogueController.Location.top);
        RectTransform panelElement = GameObject.FindObjectsOfType<AttackSurfaceUIElement>()
            .Where(element => element.elementName == "outerLatch").Select(element => element.GetComponent<RectTransform>()).First();
        GameManager.I.uiController.DrawLine(panelElement, Vector3.zero, IndicatorUIController.Origin.bottom);
        yield return ShowCutsceneDialogue("the mentor", mentorPortrait, "Most people see a locked door and reach for the lockpick, but that's a rookie mistake. The latch on this door is a weak point.", location: CutsceneDialogueController.Location.top);
        GameManager.I.uiController.DrawLine(GameManager.I.uiController.indicatorUIController.probeIndicator, Vector3.zero, IndicatorUIController.Origin.bottom);
        yield return ShowCutsceneDialogue("the mentor", mentorPortrait, "A well-placed shim can open most doors faster and quieter than any lockpick.", location: CutsceneDialogueController.Location.top);
        burglarCanvas.PreventButtons(false);
        inputProfile = InputProfile.allowAll;
        GameManager.I.uiController.ShowTutorialText("Use the probe tool on the latch");
        yield return WaitForTrigger("latch_bypass", "doorknob");
        inputProfile = InputProfile.allowNone;
        burglarCanvas.PreventButtons(true);
        yield return ShowCutsceneDialogue("the mentor", mentorPortrait, "Nicely done. Remember, simpler is better in this line of work.");
        yield return MyLeaveFocus();
        inputProfile = InputProfile.allowAll;
        GameManager.I.uiController.HideInteractiveHighlight();
        burglarCanvas.PreventButtons(false);
        burglarCanvas.PreventClose(false);
        GameManager.I.CloseBurglar();
    }

    IEnumerator KeyWatcher() {
        yield return WaitForTrigger("got_key");
        yield return MyWaitForFocus();
        Time.timeScale = 1f;
        playerCharacterController.TransitionToState(CharacterState.normal);
        yield return ShowCutsceneDialogue("the mentor", mentorPortrait, "Good looking out. That key is exactly what we need.");
        hasKey = true;
        yield return MyLeaveFocus();
        GameManager.I.uiController.HideInteractiveHighlight();
    }

    IEnumerator LootWatcher() {
        yield return WaitForTrigger("got_loot");
        yield return MyWaitForFocus();
        Time.timeScale = 1f;
        playerCharacterController.TransitionToState(CharacterState.normal);
        yield return ShowCutsceneDialogue("the mentor", mentorPortrait, "Grabbing some loot, huh? Good idea. I know a guy back on shakedown who'll want to buy that from you.");
        yield return MyLeaveFocus();
        GameManager.I.uiController.HideInteractiveHighlight();
    }

    IEnumerator Interior() {
        phase = Phase.loot;

        GameManager.I.uiController.keyMenuButtonGroup.interactable = false;
        HideBasicUI();
        GameManager.I.uiController.HideInteractiveHighlight();

        Coroutine waitForKey = CutsceneManager.I.StartCoroutine(KeyWatcher());
        Coroutine waitForLoot = CutsceneManager.I.StartCoroutine(LootWatcher());

        yield return MoveCharacter(mentorController, "mentor_interior");
        HighlightLocation("interior");
        yield return WaitForTrigger("interior");
        HideLocationHighlight();

        yield return MyWaitForFocus();
        Time.timeScale = 1f;
        playerCharacterController.TransitionToState(CharacterState.normal);
        inputProfile = inputProfile with { allowCameraControl = true };
        yield return new WaitForSecondsRealtime(0.2f);
        yield return ShowCutsceneDialogue("the mentor", mentorPortrait, "Okay. We know where the target is, so let's get moving.");
        yield return MyLeaveFocus();
        // start prevent key
        GameManager.I.uiController.HideInteractiveHighlight();
        CutsceneManager.I.StartCoroutine(Toolbox.ChainCoroutines(
            MoveCharacter(mentorController, "mentor_labdoor", speedCoefficient: 0.75f),
            Toolbox.CoroutineFunc(() => CharacterLookAt(mentorController, "labdoor"))
        ));
        HighlightLocation("labdoor");
        yield return WaitForTrigger("labdoor");
        HideLocationHighlight();
        yield return MyWaitForFocus();
        Time.timeScale = 1f;
        playerCharacterController.TransitionToState(CharacterState.normal);
        inputProfile = inputProfile with { allowCameraControl = true };
        if (hasKey) {

        } else {

            playerCharacterController.TransitionToState(CharacterState.normal);
            yield return ShowCutsceneDialogue("the mentor", mentorPortrait, "This door is locked too. I think I saw the key back where we came in, go get it.");
            yield return LeaveFocus();
            GameManager.I.uiController.HideInteractiveHighlight();

            GameManager.I.uiController.ShowTutorialText("Find the key and click to pick it up");
            yield return WaitForTrigger("got_key");
            HighlightLocation("labdoor");
            yield return WaitForTrigger("labdoor");
            yield return MyWaitForFocus();
            HideLocationHighlight();
        }
        GameManager.I.uiController.HideInteractiveHighlight();
        Time.timeScale = 1f;
        inputProfile = InputProfile.allowAll with {
            allowPlayerMovement = false,
            allowBurglarButton = false,
            // allowCameraControl = false
        };
        GameManager.I.uiController.ShowUI();
        GameManager.I.uiController.ShowTutorialText("Click the door");
        playerCharacterController.TransitionToState(CharacterState.normal);
        // stop prevent key
        yield return WaitForTrigger("locked_door");
        GameManager.I.uiController.ShowStatusBar(true);
        GameManager.I.uiController.HideCutsceneDialogue();
        yield return new WaitForSecondsRealtime(0.5f);
        GameManager.I.uiController.DrawLine(GameManager.I.uiController.indicatorUIController.keyMenuIndicator, Vector3.zero);
        yield return ShowCutsceneDialogue("the mentor", mentorPortrait, "The key menu opened up here. Let's try that key in the door.");
        GameManager.I.uiController.keyMenuButtonGroup.interactable = true;

        GameManager.I.uiController.ShowIndicator(GameManager.I.uiController.indicatorUIController.keyMenuIndicator, Vector3.left * 20f, IndicatorUIController.Direction.right);

        GameManager.I.uiController.ShowTutorialText("Click the door to reveal the key menu\nClick the key button to use the key in the lock");
        yield return WaitForTrigger("key_menu_used");
        GameManager.I.uiController.ShowTutorialText("");

        GameManager.I.uiController.HideCutsceneDialogue();
        GameManager.I.uiController.HideAllIndicators();
        GameManager.I.uiController.keyMenuButtonGroup.interactable = false;

        yield return ShowCutsceneDialogue("the mentor", mentorPortrait, "Using a key draws a lot less attention. Makes it look like you belong there. Guards hassle you less.");
        yield return ShowCutsceneDialogue("the mentor", mentorPortrait, "Even just knowing the key code helps sometimes. Some runners like to cut their own keys or keycards on a mission.");
        inputProfile = inputProfile with { allowCameraControl = true };
        yield return LeaveFocus();
        GameManager.I.uiController.keyMenuButtonGroup.interactable = true;


        CutsceneManager.I.StartCoroutine(MoveCharacter(mentorController, "mentor_lab", speedCoefficient: 0.75f));
        if (waitForKey != null) {
            CutsceneManager.I.StopCoroutine(waitForKey);
        }
        if (waitForLoot != null) {
            CutsceneManager.I.StopCoroutine(waitForLoot);
        }
    }

    IEnumerator Laboratory() {
        Coroutine wanderPrevention = CutsceneManager.I.StartCoroutine(Toolbox.RunJobRepeatedly(() => LabWanderPrevention()));

        HighlightLocation("lab");
        yield return WaitForTrigger("lab");
        HideLocationHighlight();

        yield return MyWaitForFocus();
        playerCharacterController.TransitionToState(CharacterState.normal);
        GameManager.I.uiController.ShowOverlayControls(true);
        Time.timeScale = 1f;
        inputProfile = InputProfile.allowNone;
        GameManager.I.uiController.keyMenuButtonGroup.interactable = false;

        yield return ShowCutsceneDialogue("the mentor", mentorPortrait, "Here we are. Get out your cyberdeck.");
        inputProfile = inputProfile with {
            allowePlayerItemSelect = true,
            allowCameraControl = true
        };
        GameManager.I.uiController.ShowTutorialText("Hold X to open inventory menu. Select cyberdeck and release X.");
        GameManager.I.uiController.keyMenuButtonGroup.interactable = true;

        yield return WaitForTrigger("item_select_deck");
        inputProfile = InputProfile.allowNone with {
            allowCameraControl = true
        };
        GameManager.I.uiController.HideCutsceneDialogue();
        GameManager.I.uiController.ShowTutorialText("Click the datastore node <sprite name=\"data_objective\"> to connect to it.");
        yield return WaitForTrigger("node_select_terminal");
        GameManager.I.uiController.overlayGroup.interactable = false;

        GameManager.I.uiController.ShowTutorialText("");
        GameManager.I.uiController.HideCutsceneText();

        yield return new WaitForSecondsRealtime(1f);

        GameManager.I.uiController.DrawLine(GameManager.I.uiController.indicatorUIController.lockedCybernodeIndicator, Vector3.zero);
        GameManager.I.uiController.keyMenuButtonGroup.interactable = false;

        yield return ShowCutsceneDialogue("the mentor", mentorPortrait, "The datastore is locked. We need to unlock it before we can download the objective data.");
        GameManager.I.uiController.HideAllIndicators();
        yield return ShowCutsceneDialogue("the mentor", mentorPortrait, "You can unlock cyber nodes with password data <sprite name=\"data_password\">, but we don't have any. Let's crack it with your cyberdeck.");
        GameManager.I.uiController.overlayGroup.interactable = true;
        GameManager.I.uiController.keyMenuButtonGroup.interactable = true;

        GameManager.I.uiController.ShowIndicator(GameManager.I.uiController.overlayHandler.cyberOverlay.hackTerminalController.selectorIndicators["tumbler"], Vector3.left * 20f, IndicatorUIController.Direction.right);
        GameManager.I.uiController.ShowTutorialText("Select the tumbler software");
        yield return WaitForTrigger("software_view_tumbler");
        GameManager.I.uiController.HideAllIndicators();
        GameManager.I.uiController.ShowTutorialText("Deploy the tumbler software");
        GameManager.I.uiController.ShowIndicator(GameManager.I.uiController.indicatorUIController.deployButtonIndicator, Vector3.zero, IndicatorUIController.Direction.right);
        yield return WaitForTrigger("software_deploy_tumbler");
        GameManager.I.uiController.HideCutsceneText();
        GameManager.I.uiController.HideAllIndicators();
        yield return WaitForTrigger("software_complete_tumbler");
        GameManager.I.uiController.overlayGroup.interactable = false;

        inputProfile = inputProfile with {
            allowCameraControl = false
        };
        yield return new WaitForSecondsRealtime(1f);
        GameManager.I.uiController.keyMenuButtonGroup.interactable = false;
        yield return ShowCutsceneDialogue("the mentor", mentorPortrait, "The node's wide open, let's grab that datafile.");
        GameManager.I.uiController.overlayGroup.interactable = true;
        GameManager.I.uiController.keyMenuButtonGroup.interactable = true;

        GameManager.I.uiController.ShowIndicator(GameManager.I.uiController.indicatorUIController.downloadButtonIndicator, Vector3.zero, IndicatorUIController.Direction.up);
        GameManager.I.uiController.ShowTutorialText("Click the download button");

        yield return WaitForTrigger("download_started");
        GameManager.I.uiController.HideCutsceneText();
        GameManager.I.uiController.HideAllIndicators();

        // stop rain
        rainParticles.SetActive(false);
        foreach (AmbientZone zone in rainAmbience) {
            zone.enabled = false;
        }
        yield return WaitForTrigger("download_complete");
        if (wanderPrevention != null) CutsceneManager.I.StopCoroutine(wanderPrevention);
    }

    IEnumerator Conclusion() {
        if (gunPrevention != null) {
            CutsceneManager.I.StopCoroutine(gunPrevention);
        }
        Time.timeScale = 1f;
        yield return new WaitForSecondsRealtime(1f);
        GameManager.I.uiController.keyMenuButtonGroup.interactable = false;

        yield return ShowCutsceneDialogue("the mentor", mentorPortrait, "It's in the bag. Let's get out of here. I'll go ahead- you stay here and wait for my signal.");
        GameManager.I.SetOverlay(OverlayType.none);

        GameManager.I.uiController.overlayHandler.CloseButtonCallback();
        playerCharacterController.itemHandler.ClearItem();

        vehicleObject.SetActive(true);
        GameObject npc1Obj = CutsceneManager.I.SpawnNPC("npc_spawn_1", guardTemplate);
        GameObject npc2Obj = CutsceneManager.I.SpawnNPC("npc_spawn_2", guardTemplate);

        CharacterController npc1 = npc1Obj.GetComponent<CharacterController>();
        CharacterController npc2 = npc2Obj.GetComponent<CharacterController>();
        CharacterLookAt(npc1, "mentor_escape");
        CharacterLookAt(npc2, "mentor_escape");

        yield return new WaitForSecondsRealtime(0.5f);
        yield return RotateIsometricCamera(IsometricOrientation.SE, mentorController.transform.position); // SE
        yield return MoveCharacter(mentorController, "mentor_escape", speedCoefficient: 0.5f, cameraFollows: true);
        spotlight.enabled = true;
        CharacterLookAt(mentorController, "npc_spawn_1");

        // stop music
        MusicController.I.Stop();

        yield return new WaitForSecondsRealtime(2f);
        yield return MoveCamera("ambush", 0.5f, CameraState.free);
        yield return new WaitForSecondsRealtime(0.5f);
        yield return ShowCutsceneDialogue("security", guardPortrait, "Fokion Hatzopolos! Put your hands up and don't move!");

        Coroutine pointGun1 = null;
        Coroutine pointGun2 = null;

        Coroutine npc1Move = CutsceneManager.I.StartCoroutine(
            Toolbox.ChainCoroutines(
                MoveCharacter(npc1, "guard_approach1", speedCoefficient: 1f),
                Toolbox.CoroutineFunc(() => npc1Move = null),
                Toolbox.CoroutineFunc(() => pointGun1 = CutsceneManager.I.StartCoroutine(PointGun(npc1)))
            )
        );
        yield return new WaitForSecondsRealtime(0.2f);
        Coroutine npc2Move = CutsceneManager.I.StartCoroutine(
            Toolbox.ChainCoroutines(
                MoveCharacter(npc2, "guard_approach2", speedCoefficient: 1f),
                Toolbox.CoroutineFunc(() => npc2Move = null),
                Toolbox.CoroutineFunc(() => pointGun2 = CutsceneManager.I.StartCoroutine(PointGun(npc2)))
            )
        );
        yield return new WaitForSecondsRealtime(1f);

        // start theme music
        MusicController.I.PlayMultiple(
            new SimpleMusicController(MusicTrack.sympatheticDetonation, MusicController.I.audioSources),
            new SimpleMusicController(MusicTrack.theme, MusicController.I.audioSources, looping: false)
        );

        yield return MoveCamera("mentor_escape", 0.5f, CameraState.normal);
        yield return new WaitForSecondsRealtime(0.25f);
        yield return ShowCutsceneDialogue("the mentor", mentorPortrait, "Kid, they don't know you're here, I work alone. Stay out of sight and escape when you see an opening.");

        while (npc2Move != null) {
            CameraInput camInput = new CameraInput {
                deltaTime = Time.unscaledDeltaTime,
                wallNormal = Vector2.zero,
                lastWallInput = Vector2.zero,
                crouchHeld = false,
                cameraState = CameraState.normal,
                targetData = CursorData.none,
                playerDirection = playerCharacterController.direction,
                popoutParity = PopoutParity.left,
                ignoreAttractor = true,
                targetPosition = npc2.transform.position + Vector3.up,
                cullingTargetPosition = npc2.transform.position,
            };
            characterCamera.UpdateWithInput(camInput);
            characterCamera.SetInputs(PlayerInput.none);
            yield return null;
        }

        yield return new WaitForSecondsRealtime(1f);
        yield return ShowCutsceneDialogue("security", guardPortrait, "You're surrounded, Hatzopolos. Don't try anything.");
        yield return MoveCamera("mentor_escape", 0.5f, CameraState.normal);
        yield return ShowCutsceneDialogue("the mentor", mentorPortrait, "You boys can relax, alright? I'm not armed.");

        yield return MoveCamera("router2", 0.5f, CameraState.normal);
        yield return ShowCutsceneDialogue("jack", jackPortrait, "Shit!");
        yield return RotateIsometricCamera(IsometricOrientation.SW, mentorController.transform.position); // SE, NE
        yield return MoveCharacter(playerCharacterController, "player_escape", speedCoefficient: 1f, cameraFollows: true);

        if (pointGun1 != null)
            CutsceneManager.I.StopCoroutine(pointGun1);
        if (pointGun2 != null)
            CutsceneManager.I.StopCoroutine(pointGun2);
        yield return CameraIsometricZoom(2f, 0.5f);

        GameManager.I.uiController.ShowInteractiveHighlight();
        inputProfile = InputProfile.allowAll with {
            allowPlayerMovement = false
        };
        GameManager.I.uiController.ShowTutorialText("Mouse over the air duct and click the <sprite name=\"screwdriver\"> icon to open the burglar view");
        closetDoor.CloseDoor();

        BurglarCanvasController burglarCanvas = null;
        while (burglarCanvas == null) {
            burglarCanvas = GameObject.FindObjectOfType<BurglarCanvasController>(true);
            yield return null;
        }

        burglarCanvas.PreventClose(true);
        yield return WaitForTrigger("burglar_open");
        GameManager.I.uiController.ShowTutorialText("Unscrew all 4 screws");
        inputProfile = InputProfile.allowAll;

        yield return WaitForTrigger("unscrew");
        yield return WaitForTrigger("unscrew");
        yield return WaitForTrigger("unscrew");
        yield return WaitForTrigger("unscrew");
        GameManager.I.uiController.ShowTutorialText("With hand tool <sprite name=\"hand\"> selected, remove the vent cover then click the air duct");
        yield return WaitForTrigger("enter_hvac");
        GameManager.I.uiController.ShowTutorialText("Crawl with WASD");
        burglarCanvas.PreventClose(false);
        yield return WaitForTrigger("airduct");

        inputProfile = InputProfile.allowNone;
        GameManager.I.uiController.ShowTutorialText("");

        ScriptSceneLocation data = CutsceneManager.I.worldLocations["airduct_target"];
        Quaternion direction = Quaternion.LookRotation(data.transform.position - playerCharacterController.transform.position, Vector3.up);
        yield return MoveCamera(playerCharacterController.transform.position, direction, 0.25f, CameraState.free);
        playerCharacterController.TransitionToState(CharacterState.hvacAim);

        CharacterLookAt(playerCharacterController, "airduct_target");
        yield return null;
        CharacterLookAt(playerCharacterController, "airduct_target");
        yield return null;
        CharacterLookAt(playerCharacterController, "airduct_target");
        yield return null;
        yield return new WaitForSecondsRealtime(0.2f);

        bool trigger = false;
        Action<string> callback = (string triggerId) => {
            trigger |= triggerId == "airduct_target";
        };
        CutsceneManager.OnTrigger += callback;
        while (!trigger) {
            PlayerInput input = PlayerInput.none;
            input.MoveAxisForward = 1;
            playerCharacterController.SetInputs(input);
            CameraInput cameraInput = playerCharacterController.BuildCameraInput();
            characterCamera.UpdateWithInput(cameraInput);
            yield return null;
        }
        CutsceneManager.OnTrigger -= callback;

        Time.timeScale = 1f;
        GameManager.I.doPlayerInput = false;
        yield return new WaitForSecondsRealtime(1f);
        yield return FreeLookToward(playerCharacterController, "airduct_look");

        yield return ShowCutsceneDialogue("security", guardPortrait, "Time's up, Hatzopolos. We're here to collect. Where's the device?");
        yield return ShowCutsceneDialogue("the mentor", mentorPortrait, "I don't know what you're talking about.");
        GameManager.I.doPlayerInput = true;

        GunHandler gunHandler1 = npc1Obj.GetComponentInChildren<GunHandler>();
        TaskShoot shoot1 = new TaskShoot(gunHandler1);
        shoot1.SetData("lastSeenPlayerPosition", mentorObject.transform.position + Vector3.up);
        CharacterHurtable mentorHurtable = mentorObject.GetComponentInChildren<CharacterHurtable>();
        while (mentorHurtable.hitState != HitState.dead) {
            PlayerInput input = PlayerInput.none;
            shoot1.DoEvaluate(ref input);
            // npc1.SetInputs(input);
            npc2.SetInputs(input);
            yield return null;
        }

        yield return new WaitForSecondsRealtime(2f);
        yield return ShowCutsceneDialogue("jack", jackPortrait, "Shit!");
        GameManager.I.uiController.keyMenuButtonGroup.interactable = true;

        yield return LeaveFocus();
        inputProfile = InputProfile.allowAll;

        yield return WaitForTrigger("airduct_exit");
        GameManager.I.uiController.ShowTutorialText("Press Tab to switch camera mode");
        yield return WaitForTrigger("airduct_exit_2");
        GameManager.I.uiController.ShowTutorialText("");
    }

    IEnumerator Escape() {
        yield return WaitForTrigger("exit");
        yield return WaitForFocus();
        playerCharacterController.TransitionToState(CharacterState.normal);
        Time.timeScale = 1f;
        GameManager.I.doPlayerInput = false;
        yield return MoveCharacter(playerCharacterController, "exitWalk", speedCoefficient: 1.2f);
    }

    IEnumerator FreeLookToward(CharacterController controller, string key) {
        ScriptSceneLocation data = CutsceneManager.I.worldLocations[key];
        Vector3 targetPosition = data.transform.position;
        Vector3 initialPosition = controller.transform.position + controller.transform.forward;
        yield return Toolbox.Ease(null, 1f, 0f, 1f, PennerDoubleAnimation.Linear, (amount) => {
            Vector3 target = Vector3.Lerp(initialPosition, targetPosition, amount);
            CharacterSnapFreeLookAt(playerCharacterController, target);
        }, unscaledTime: true);
    }

    IEnumerator PointGun(CharacterController controller) {
        PlayerInput input = PlayerInput.none;
        input.aimWeapon = true;
        input.lookAtPosition = mentorController.transform.position;
        input.orientTowardPoint = mentorController.transform.position;
        input.snapToLook = true;
        while (true) {
            controller.SetInputs(input);
            yield return null;
        }
    }
    protected IEnumerator MyLeaveFocus() {
        yield return CutsceneManager.I.LeaveFocus(this);
        HideBasicUI();
    }
    protected IEnumerator MyWaitForFocus() {
        yield return WaitForFocus();
        HideBasicUI();
    }
    void HideBasicUI() {
        GameManager.I.uiController.ShowStatusBar(false);
        GameManager.I.uiController.ShowAppearanceInfo(false);
        GameManager.I.uiController.ShowVisibilityInfo(false);
        GameManager.I.uiController.ShowOverlayControls(false);
        switch (phase) {
            case Phase.network:
                GameManager.I.uiController.ShowOverlayControls(true);
                break;
            case Phase.stealth:
                GameManager.I.uiController.ShowOverlayControls(true);
                GameManager.I.uiController.ShowAppearanceInfo(true);
                GameManager.I.uiController.ShowVisibilityInfo(true);
                break;
            case Phase.loot:
                GameManager.I.uiController.ShowOverlayControls(true);
                GameManager.I.uiController.ShowAppearanceInfo(true);
                GameManager.I.uiController.ShowVisibilityInfo(true);
                GameManager.I.uiController.ShowStatusBar(true);
                break;
        }
    }
}