using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI {
    public class TaskOpenDialogue : TaskNode {
        public TaskOpenDialogue() : base() {
        }
        public override void Initialize() {
            base.Initialize();
            GameManager.I.ShowMenu(MenuType.dialogue);
            // get reference to dialogue menu
            // subscribe to dialogue end method
            // track when dialogue concludes with result
        }
        public override TaskState DoEvaluate(ref PlayerInput input) {
            return TaskState.running;
        }
    }
}
