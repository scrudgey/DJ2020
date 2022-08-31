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
            input.orientTowardPoint.y = 0;
            input.Fire = fireData;
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
            CursorData cursorData = new CursorData {
                type = CursorData.TargetType.objectLock,
                screenPosition = Vector2.zero,
                screenPositionNormalized = Vector2.zero,
                highlightableTargetData = null,
                worldPosition = lastSeenPlayerPosition + new Vector3(0f, 0.4f, 0f),
                mousePosition = lastSeenPlayerPosition
            };
            bool clearshot = gunHandler.IsClearShot(cursorData);

            PlayerInput.FireInputs fireInput = clearshot ? new PlayerInput.FireInputs() {
                FirePressed = false,
                FireHeld = true,
                cursorData = cursorData
            } : PlayerInput.FireInputs.none;

            return fireInput;
        }
    }
}
