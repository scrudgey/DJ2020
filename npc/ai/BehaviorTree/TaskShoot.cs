using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI {
    public class TaskShoot : TaskNode {
        readonly float SHOOT_INTERVAL = 0.1f;
        float shootTimer;
        GunHandler gunHandler;
        public TaskShoot(GunHandler gunHandler) : base() {
            this.gunHandler = gunHandler;
        }
        public override TaskState DoEvaluate(ref PlayerInput input) {
            shootTimer += Time.deltaTime;
            if (shootTimer > SHOOT_INTERVAL) {
                shootTimer -= SHOOT_INTERVAL;
                PlayerInput.FireInputs fireData = ShootBullet();
                input.lookAtPoint = fireData.targetData.position;
                input.Fire = fireData;
            }
            return TaskState.running;
        }
        PlayerInput.FireInputs ShootBullet() {
            Vector3 lastSeenPlayerPosition = (Vector3)GetData("lastSeenPlayerPosition");
            if (lastSeenPlayerPosition == null)
                return new PlayerInput.FireInputs();
            PlayerInput.FireInputs fireInput = new PlayerInput.FireInputs() {
                FirePressed = false,
                FireHeld = true,
                targetData = new TargetData2 {
                    type = TargetData2.TargetType.objectLock,
                    screenPosition = Vector3.zero,
                    highlightableTargetData = null,
                    position = lastSeenPlayerPosition
                }
            };
            return fireInput;
        }
    }
}
