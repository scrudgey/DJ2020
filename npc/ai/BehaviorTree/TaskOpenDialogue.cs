using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AI {
    public class TaskOpenDialogue : TaskNode {
        bool concluded = false;
        UIController uiController;
        public TaskOpenDialogue() : base() {
        }
        public override void Initialize() {
            base.Initialize();
            // Yikesaroo! Hacky BS!
            if (!SceneManager.GetSceneByName("DialogueMenu").isLoaded) {
                GameManager.I.LoadScene("DialogueMenu", () => {
                    DialogueController menuController = GameObject.FindObjectOfType<DialogueController>();
                    uiController = GameObject.FindObjectOfType<UIController>();
                    Time.timeScale = 0f;
                    menuController.Initialize();
                    uiController.HideUI();
                    menuController.OnDialogueConclude += HandleDialogueResult;
                    Debug.Log($"loaded dialogue menu finish callback {menuController}");
                }, unloadAll: false);
            }
        }
        public override TaskState DoEvaluate(ref PlayerInput input) {
            if (concluded) {
                return TaskState.success;
            } else {
                return TaskState.running;
            }
        }

        public void HandleDialogueResult(DialogueController.DialogueResult result) {
            concluded = true;
            Time.timeScale = 1f;
            uiController.ShowUI();
            Scene sceneToUnload = SceneManager.GetSceneByName("DialogueMenu");
            SceneManager.UnloadSceneAsync(sceneToUnload);
        }
    }
}
