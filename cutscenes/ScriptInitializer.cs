using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class ScriptInitializer : MonoBehaviour {
    public GameObject truckPrefab;
    public NPCTemplate driverTemplate;
    public NPCRandomTemplate[] punkTemplates;
    public Sprite driverPortrait;
    public Sprite communicationPortrait;
    bool scriptStarted;
    public void StartCutscene() {
        if (scriptStarted) {
            return;
        }
        CutsceneManager.I.StartCutscene(new TruckCutscene(truckPrefab, driverTemplate, punkTemplates, driverPortrait));
        CutsceneManager.I.StartCutscene(new AbandonedLabIntroCutscene(communicationPortrait));
        scriptStarted = true;
    }
}

