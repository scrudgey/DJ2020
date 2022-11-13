using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AI {
    public class TaskOpenDialogue : TaskNode {
        bool isConcluded = false;
        SphereRobotAI ai;
        // Action<DialogueController.DialogueResult> callback;
        public TaskOpenDialogue(SphereRobotAI ai) : base() {
            this.ai = ai;
            // this.callback = callback;
        }
        public override void Initialize() {
            base.Initialize();
            // Yikesaroo! Hacky BS!
            if (!SceneManager.GetSceneByName("DialogueMenu").isLoaded) {
                DialogueInput input = ai.GetDialogueInput();
                GameManager.I.LoadScene("DialogueMenu", () => {
                    DialogueController menuController = GameObject.FindObjectOfType<DialogueController>();
                    menuController.Initialize(input);
                    GameManager.I.uiController.HideUI();
                    DialogueController.OnDialogueConclude += HandleDialogueResult;
                    // if (callback != null)
                    // menuController.OnDialogueConclude += callback;
                    Debug.Log($"loaded dialogue menu finish callback {menuController}");
                }, unloadAll: false);
            }
        }
        public override TaskState DoEvaluate(ref PlayerInput input) {
            if (isConcluded) {
                return TaskState.success;
            } else {
                return TaskState.running;
            }
        }

        public void HandleDialogueResult(DialogueController.DialogueResult result) {
            DialogueController.OnDialogueConclude -= HandleDialogueResult;
            isConcluded = true;
            Time.timeScale = 1f;
            GameManager.I.uiController.ShowUI();
            Scene sceneToUnload = SceneManager.GetSceneByName("DialogueMenu");
            SceneManager.UnloadSceneAsync(sceneToUnload);
        }

        public override void Reset() {
            base.Reset();
            isConcluded = false;
        }
    }
}
