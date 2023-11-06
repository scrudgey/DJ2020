using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AI {
    public class TaskOpenDialogue : TaskNode {
        bool isConcluded = false;
        DialogueCharacterInput characterInput;
        GameObject gameObject;
        public TaskOpenDialogue(GameObject gameObject, DialogueCharacterInput characterInput) : base() {
            this.gameObject = gameObject;
            this.characterInput = characterInput;
        }
        public override void Initialize() {
            base.Initialize();
            if (GameManager.I.activeMenuType != MenuType.dialogue) {
                // DialogueInput input = ai.GetDialogueInput();
                DialogueInput input = GameManager.I.GetDialogueInput(gameObject, characterInput);
                GameManager.I.ShowMenu(MenuType.dialogue, () => {
                    // DialogueController menuController = GameObject.FindObjectOfType<DialogueController>();
                    NeoDialogueMenu menuController = GameObject.FindObjectOfType<NeoDialogueMenu>();
                    // menuController.Initialize(input);
                    menuController.Initialize(input, HandleDialogueResult);
                    // DialogueController.OnDialogueConclude += HandleDialogueResult;
                    // NeoDialogueMenu.OnDialogueConclude += HandleDialogueResult;
                });
            }
        }
        public override TaskState DoEvaluate(ref PlayerInput input) {
            if (isConcluded) {
                return TaskState.success;
            } else {
                return TaskState.running;
            }
        }

        public void HandleDialogueResult(NeoDialogueMenu.DialogueResult result) {
            // DialogueController.OnDialogueConclude -= HandleDialogueResult;
            // NeoDialogueMenu.OnDialogueConclude -= HandleDialogueResult;
            isConcluded = true;
        }

        public override void Reset() {
            base.Reset();
            isConcluded = false;
        }
    }
}
