using System.Collections;
using System.Collections.Generic;
using AI;
using UnityEngine;

namespace AI {

    public class TaskCower : TaskNode {
        public float headSwivelOffset;
        Vector3 baseLookDirection;
        public TaskCower() : base() {
            baseLookDirection = Random.insideUnitSphere;
            baseLookDirection.y = 0;
        }
        public override TaskState DoEvaluate(ref PlayerInput input) {
            headSwivelOffset = 45f * Mathf.Sin(Time.time * 2f);
            Vector3 lookDirection = Quaternion.AngleAxis(headSwivelOffset, Vector3.up) * baseLookDirection;
            input.lookAtDirection = lookDirection;
            input.orientTowardDirection = baseLookDirection;
            input.CrouchDown = true;
            return TaskState.running;
        }

    }

}