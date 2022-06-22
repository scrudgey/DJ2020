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
            input.orientTowardPoint = fireData.cursorData.worldPosition;
            input.Fire = fireData;
            input.inputMode = InputMode.gun;
            input.lookAtPosition = fireData.cursorData.worldPosition;
            return TaskState.running;
        }
        PlayerInput.FireInputs ShootBullet() {
            object lastSeenPlayerPositionObject = GetData("lastSeenPlayerPosition");
            if (lastSeenPlayerPositionObject == null)
                return PlayerInput.FireInputs.none;
            Vector3 lastSeenPlayerPosition = (Vector3)lastSeenPlayerPositionObject;
            if (lastSeenPlayerPosition == null)
                return PlayerInput.FireInputs.none;
            PlayerInput.FireInputs fireInput = new PlayerInput.FireInputs() {
                FirePressed = false,
                FireHeld = true,
                cursorData = new CursorData {
                    type = CursorData.TargetType.objectLock,
                    screenPosition = Vector2.zero,
                    screenPositionNormalized = Vector2.zero,
                    highlightableTargetData = null,
                    worldPosition = lastSeenPlayerPosition
                }
            };
            return fireInput;
        }
    }
}
