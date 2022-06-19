using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI {
    public class TaskShoot : TaskNode {
        GunHandler gunHandler;
        public TaskShoot(GunHandler gunHandler) : base() {
            this.gunHandler = gunHandler;
        }
        public override TaskState DoEvaluate(ref PlayerInput input) {
            PlayerInput.FireInputs fireData = ShootBullet();
            input.lookAtPoint = fireData.targetData.position;
            input.Fire = fireData;
            input.inputMode = InputMode.gun;
            return TaskState.running;
        }
        PlayerInput.FireInputs ShootBullet() {
            object lastSeenPlayerPositionObject = GetData("lastSeenPlayerPosition");
            if (lastSeenPlayerPositionObject == null)
                return new PlayerInput.FireInputs();
            Vector3 lastSeenPlayerPosition = (Vector3)lastSeenPlayerPositionObject;
            if (lastSeenPlayerPosition == null)
                return new PlayerInput.FireInputs();
            PlayerInput.FireInputs fireInput = new PlayerInput.FireInputs() {
                FirePressed = false,
                FireHeld = true,
                targetData = new TargetData2 {
                    type = TargetData2.TargetType.objectLock,
                    screenPosition = Vector2.zero,
                    screenPositionNormalized = Vector2.zero,
                    highlightableTargetData = null,
                    position = lastSeenPlayerPosition
                }
            };
            return fireInput;
        }
    }
}
