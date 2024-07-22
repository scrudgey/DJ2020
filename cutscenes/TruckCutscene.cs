using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class TruckCutscene : Cutscene {
    GameObject truckPrefab;
    NPCTemplate driverTemplate;
    NPCRandomTemplate[] punkTemplates;
    Sprite driverPortrait;
    public TruckCutscene(GameObject truckPrefab, NPCTemplate driverTemplate, NPCRandomTemplate[] punkTemplates, Sprite driverPortrait) {
        this.truckPrefab = truckPrefab;
        this.punkTemplates = punkTemplates;
        this.driverTemplate = driverTemplate;
        this.driverPortrait = driverPortrait;
    }

    public override IEnumerator DoCutscene() {
        yield return CutsceneManager.WaitForTrigger("enterLab");
        yield return new WaitForSeconds(20f);

        CharacterController driver = null;
        CharacterController[] characters = new CharacterController[5];
        GameObject truckObj = CutsceneManager.I.SpawnObject("truckEnter", truckPrefab, "truckDriveTo");
        Vehicle truck = truckObj.GetComponent<Vehicle>();
        for (int i = 0; i < 5; i++) {
            GameObject npcObj = null;

            if (i == 0) {
                npcObj = CutsceneManager.I.SpawnNPC("npcSpawn", driverTemplate);
            } else {
                NPCRandomTemplate randomTemplate = Toolbox.RandomFromList(punkTemplates);
                NPCTemplate template = randomTemplate.toTemplate();
                npcObj = CutsceneManager.I.SpawnNPC("npcSpawn", template);
            }


            CharacterController controller = npcObj.GetComponent<CharacterController>();
            truck.LoadCharacter(controller);
            if (i == 0) {
                driver = controller;
            }
            characters[i] = controller;
        }

        yield return WaitForFocus();
        GameManager.I.uiController.ShowCutsceneText("warning");
        yield return new WaitForSecondsRealtime(1f);

        yield return MoveCamera("truck1", 1f, CameraState.free);

        yield return truck.DriveToPoint("truckDriveTo");

        GameManager.I.uiController.HideCutsceneText();
        yield return new WaitForSecondsRealtime(2f);

        SetCameraPosition("truck2", CameraState.free);
        CutsceneManager.I.NPCLookAt(driver, "door");

        yield return new WaitForSecondsRealtime(2f);
        yield return GameManager.I.uiController.ShowCutsceneDialogue("punk", driverPortrait, "Check it out! The old warehouse is open!");
        yield return MoveCamera("door", 1f, CameraState.free);
        yield return new WaitForSecondsRealtime(1.75f);
        yield return MoveCamera("truck3", 1f, CameraState.free);
        WaitForSecondsRealtime waiter = new WaitForSecondsRealtime(0.4f);
        yield return new WaitForSecondsRealtime(1.4f);
        for (int i = 0; i < 5; i++) {
            truck.Unload();
            CutsceneManager.I.NPCClearPoints(characters[i]);
            yield return waiter;
        }
        yield return new WaitForSecondsRealtime(2f);
        yield return GameManager.I.uiController.ShowCutsceneDialogue("punk", driverPortrait, "Looks like an old chem research lab, so watch out for drugs and toxins!");
        GameManager.I.AddSuspicionRecord(SuspicionRecord.snoopingSuspicion());
    }
}