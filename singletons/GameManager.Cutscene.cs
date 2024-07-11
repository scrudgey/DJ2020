// using System;
// using System.Collections;
// using System.Collections.Generic;
// using System.IO;
// using System.Linq;
// using Easings;
// using TMPro;
// using UnityEngine;
// using UnityEngine.SceneManagement;
// public partial class GameManager : Singleton<GameManager> {
//     public void StartSpottedCutscene(SphereRobotAI NPC) {
//         if (GameObject.FindObjectsOfType<SphereRobotAI>().Any(ai => ai.stateMachine.currentState is SphereInvestigateState)) return;
//         CutsceneManager.I.StartCutscene(new SpottedCutscene(NPC));
//     }
//     public void ShowExtractionZoneCutscene(ExtractionZone zone) {
//         CutsceneManager.I.StartCutscene(new ExtractionZoneCutscene(zone));
//     }
//     public void ShowGrateKickCutscene(HVACElement element, CharacterController controller) {
//         CutsceneManager.I.StartCutscene(new KickOutHVACGrateCutscene(element));
//     }
// }