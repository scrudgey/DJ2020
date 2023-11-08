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
        Action<NeoDialogueMenu.DialogueResult> resultCallback;
        public TaskOpenDialogue(GameObject gameObject, DialogueCharacterInput characterInput, Action<NeoDialogueMenu.DialogueResult> resultCallback) : base() {
            this.gameObject = gameObject;
            this.characterInput = characterInput;
            this.resultCallback = resultCallback;
        }
        public override void Initialize() {
            base.Initialize();
            if (GameManager.I.activeMenuType != MenuType.dialogue) {
                DialogueInput input = GameManager.I.GetDialogueInput(gameObject, characterInput);
                GameManager.I.ShowMenu(MenuType.dialogue, () => {
                    NeoDialogueMenu menuController = GameObject.FindObjectOfType<NeoDialogueMenu>();
                    menuController.Initialize(input, HandleDialogueResult);
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
            isConcluded = true;
            resultCallback.Invoke(result);
        }

        public override void Reset() {
            base.Reset();
            isConcluded = false;
        }
    }
}
