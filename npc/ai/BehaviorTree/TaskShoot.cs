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
                ShootBullet();
            }
            return TaskState.running;
        }
        void ShootBullet() {
            Vector3 lastSeenPlayerPosition = (Vector3)GetData("lastSeenPlayerPosition");
            if (lastSeenPlayerPosition == null)
                return;
            PlayerInput.FireInputs fireInput = new PlayerInput.FireInputs() {
                FirePressed = true,
                FireHeld = false,
                targetData = new TargetData2 {
                    type = TargetData2.TargetType.objectLock,
                    screenPosition = Vector3.zero,
                    highlightableTargetData = null,
                    position = lastSeenPlayerPosition
                }
            };
            gunHandler.ShootImmediately(fireInput);
        }

    }

}
