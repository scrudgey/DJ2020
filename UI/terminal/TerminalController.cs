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
        }
        public void OnEnable() {
            TakeFocus();
        }
        public void OnConsoleEditEnd(string fieldValue) {
            ReceiveInput(fieldValue.Trim());
            consoleInput.text = "";
            TakeFocus();
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
        public void SetValue(string[] args) {
            string fieldName = args[0];
            int value = int.Parse(args[1]);
            switch (fieldName) {
                case "cyberlegs":
                    GameManager.I.gameData.playerData.cyberlegsLevel = value;
                    break;
                case "debug-rays":
                    GameManager.I.showDebugRays = value == 1;
                    break;
                case "pistol-skill":
                    GameManager.I.gameData.playerData.gunSkillLevel[GunType.pistol] = value;
                    break;
                case "smg-skill":
                    GameManager.I.gameData.playerData.gunSkillLevel[GunType.smg] = value;
                    break;
                case "rifle-skill":
                    GameManager.I.gameData.playerData.gunSkillLevel[GunType.rifle] = value;
                    break;
                case "shotgun-skill":
                    GameManager.I.gameData.playerData.gunSkillLevel[GunType.shotgun] = value;
                    break;
                // case "test":
                //     TestSuite.RunToolboxTests();
                //     break;
                default:
                    Println($"unrecognized set parameter: {fieldName}");
                    break;
            }
        }

    }
}
