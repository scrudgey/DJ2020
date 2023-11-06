using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace UI {
    public class TerminalCommand {

    }
    public class TerminalController : MonoBehaviour {
        public TextMeshProUGUI consoleOutput;
        public TMP_InputField consoleInput;
        private Dictionary<string, Action<string[]>> commands = new Dictionary<string, Action<string[]>>();
        public void Start() {
            consoleInput.onEndEdit.AddListener(OnConsoleEditEnd);
            commands["set"] = SetValue;
            commands["test"] = RunTests;
            commands["alarm"] = ToggleAlarm;
            commands["disguise"] = ToggleDisguise;
            commands["timescale"] = TimeScale;
            commands["givegun"] = GiveGun;
            commands["resetPerks"] = ResetPerks;
            commands["gunskill"] = GunSkill;
            commands["completeMission"] = CompleteMission;
            commands["suspicion"] = Suspicion;
        }
        public void OnEnable() {
            TakeFocus();
        }
        public void OnConsoleEditEnd(string fieldValue) {
            TakeFocus();
            consoleInput.text = "";
            ReceiveInput(fieldValue.Trim());
        }
        public void TakeFocus() {
            consoleInput.Select();
            consoleInput.ActivateInputField();
        }
        public void ReceiveInput(string input) {
            Println(input);
            string[] rawInput = ParseCommand(input);
            string commandName = rawInput[0];
            string[] args = rawInput.Skip(1).Take(rawInput.Length - 1).ToArray();

            if (commands.ContainsKey(commandName)) {
                commands[commandName](args);
            } else {
                Println($"unrecognized command: {commandName}");
            }
        }
        public void Println(string line) {
            consoleOutput.text += "\n" + line;
        }
        public string[] ParseCommand(string input) {
            return input.Split(' ');
        }

        // commands
        public void RunTests(string[] args) {
            Println($"running test suite...");
            TestSuite.RunToolboxTests();
        }
        public void ToggleAlarm(string[] args) {
            if (GameManager.I.gameData.levelState.anyAlarmTerminalActivated())
                GameManager.I.DeactivateAlarm();
            else
                GameManager.I.ActivateHQRadioNode();

        }
        public void ToggleDisguise(string[] args) {
            bool disguise = GameManager.I.gameData.levelState.delta.disguise;
            if (disguise)
                GameManager.I.DeactivateDisguise();
            else
                GameManager.I.ActivateDisguise();
        }
        public void TimeScale(string[] args) {
            float timescale = float.Parse(args[0]);
            Time.timeScale = timescale;
        }
        public void Objectives(string[] args) {
            // float timescale = float.Parse(args[0]);
            // Time.timeScale = timescale;
            GameManager.I.gameData.levelState.delta.objectiveStatus = ObjectiveStatus.complete;
            GameManager.I.HandleAllObjectivesComplete();
        }

        public void SetValue(string[] args) {
            string fieldName = args[0];
            int value = int.Parse(args[1]);
            switch (fieldName) {
                case "cyberlegs":
                    GameManager.I.gameData.playerState.cyberlegsLevel = value;
                    break;
                case "debug-rays":
                    GameManager.I.showDebugRays = value == 1;
                    break;
                case "eyes-laser":
                    GameManager.I.gameData.playerState.cyberEyesThermal = value == 1;
                    GameManager.OnEyeVisibilityChange?.Invoke(GameManager.I.gameData.playerState);
                    break;
                default:
                    Println($"unrecognized set parameter: {fieldName}");
                    break;
            }
        }

        public void GiveGun(string[] args) {
            string templateName = args[1];
            GunTemplate gun1 = GunTemplate.Load(templateName);
            GunState gunState = GunState.Instantiate(gun1);
            switch (args[0]) {
                case "1":
                    GameManager.I.gameData.playerState.primaryGun = gunState;
                    break;
                case "2":
                    GameManager.I.gameData.playerState.secondaryGun = gunState;
                    break;
                case "3":
                    GameManager.I.gameData.playerState.tertiaryGun = gunState;
                    break;
                default:
                    Debug.LogError("bad gun slot");
                    break;
            }
            GameManager.I.playerGunHandler.LoadGunHandlerState(GameManager.I.gameData.playerState);
        }
        public void ResetPerks(string[] args) {
            GameManager.I.gameData.playerState.activePerks = new List<string>();
        }
        public void CompleteMission(string[] args) {
            GameManager.I.CloseMenu();
            GameManager.I.HandleAllObjectivesComplete();
        }
        public void Suspicion(string[] args) {
            GameManager.I.CloseMenu();
            GameManager.I.AddSuspicionRecord(SuspicionRecord.bodySuspicion());
        }
        public void GunSkill(string[] args) {
            string gunTypeString = args[0];
            int accuracy = int.Parse(args[1]);
            int control = int.Parse(args[2]);
            string[] accuracyStrings = Enumerable.Range(1, accuracy).Select(i => i.ToString()).ToArray();
            string[] controlStrings = Enumerable.Range(1, control).Select(i => i.ToString()).ToArray();

            switch (gunTypeString) {
                case "pistol":
                    foreach (string accuracyString in accuracyStrings)
                        GameManager.I.gameData.playerState.activePerks.Add($"p1_{accuracyString}");
                    foreach (string controlString in controlStrings)
                        GameManager.I.gameData.playerState.activePerks.Add($"p2_{controlString}");
                    break;
                case "smg":
                    foreach (string accuracyString in accuracyStrings)
                        GameManager.I.gameData.playerState.activePerks.Add($"smg1_{accuracyString}");
                    foreach (string controlString in controlStrings)
                        GameManager.I.gameData.playerState.activePerks.Add($"smg2_{controlString}");
                    break;
                case "shotgun":
                    foreach (string accuracyString in accuracyStrings)
                        GameManager.I.gameData.playerState.activePerks.Add($"sh1_{accuracyString}");
                    foreach (string controlString in controlStrings)
                        GameManager.I.gameData.playerState.activePerks.Add($"sh2_{controlString}");
                    break;
                case "rifle":
                    foreach (string accuracyString in accuracyStrings)
                        GameManager.I.gameData.playerState.activePerks.Add($"rifle1_{accuracyString}");
                    foreach (string controlString in controlStrings)
                        GameManager.I.gameData.playerState.activePerks.Add($"rifle2_{controlString}");
                    break;
            }
        }
    }
}
