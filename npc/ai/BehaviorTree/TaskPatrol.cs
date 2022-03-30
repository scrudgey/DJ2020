using System.Collections;
using System.Collections.Generic;
using AI;
using UnityEngine;

namespace AI {

    public class TaskPatrol : TaskNode {
        private Transform _transform;
        private Transform[] _waypoints;
        private int _currentWaypointIndex = 0;
        private float _waitTime = 1f; // in seconds
        private float _waitCounter = 0f;
        private bool _waiting = false;

        public TaskPatrol(Transform transform, Transform[] waypoints) {
            _transform = transform;
            _waypoints = waypoints;
        }

        public override TaskState Evaluate(ref PlayerInput input) {
            if (_waiting) {
                _waitCounter += Time.deltaTime;
                if (_waitCounter >= _waitTime) {
                    _waiting = false;
                }
            } else {
                Transform wp = _waypoints[_currentWaypointIndex];
                if (Vector3.Distance(_transform.position, wp.position) < 0.01f) {
                    _transform.position = wp.position;
                    _waitCounter = 0f;
                    _waiting = true;

                    _currentWaypointIndex = (_currentWaypointIndex + 1) % _waypoints.Length;
                } else {
                    // _transform.position = Vector3.MoveTowards(_transform.position, wp.position, GuardBT.speed * Time.deltaTime);
                    _transform.LookAt(wp.position);
                }
            }
            state = TaskState.running;
            return state;
        }

    }
}