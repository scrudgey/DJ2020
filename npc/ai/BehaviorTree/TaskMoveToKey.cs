using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AI;
using UnityEngine;
using UnityEngine.AI;
namespace AI {

    public class TaskMoveToKey : TaskNode {
        public enum HeadBehavior { normal, casual, search }
        public HeadBehavior headBehavior;
        float CORNER_ARRIVAL_DISTANCE = 0.15f;
        public NavMeshPath navMeshPath;
        public int pathIndex;
        Transform transform;
        string key;
        float repathTimer;
        public float repathInterval = 1f;
        int navFailures = 0;
        public float headSwivelOffset;
        public float speedCoefficient = 1f;
        Vector3 baseLookDirection;
        HashSet<int> keyIds;
        Door waitForDoor;
        public TaskMoveToKey(Transform transform, string key, HashSet<int> keyIds, float arrivalDistance = 0.15f) : base() {
            navMeshPath = new NavMeshPath();
            pathIndex = -1;
            this.transform = transform;
            this.key = key;
            this.CORNER_ARRIVAL_DISTANCE = arrivalDistance;
            this.keyIds = keyIds;
            SetDestination();
        }

        public Vector3[] GetNavPath() {
            if (pathIndex <= navMeshPath.corners.Length - 1) {
                return navMeshPath.corners[pathIndex..^0];
            } else return new Vector3[0];
        }

        public override void Initialize() {
            SetDestination();
        }
        public override TaskState DoEvaluate(ref PlayerInput input) {

            // possibly fail
            if (navFailures >= 2) {
                return TaskState.failure;
            }

            // repathing
            if (repathTimer > repathInterval) {
                repathTimer -= repathInterval;
                SetDestination();
            }
            repathTimer += Time.deltaTime;

            // Head / look direction
            if (headBehavior == HeadBehavior.casual) {
                headSwivelOffset = 45f * Mathf.Sin(Time.time);
            } else if (headBehavior == HeadBehavior.search) {
                headSwivelOffset = 45f * Mathf.Sin(Time.time * 2f);
            }
            Vector3 lookDirection = baseLookDirection;
            lookDirection = Quaternion.AngleAxis(headSwivelOffset, Vector3.up) * lookDirection;
            input.lookAtDirection = lookDirection;

            // navigation
            if (waitForDoor != null) {
                // TODO: more elaborate behavior to navigate to a specific point provided by the door.
                if (waitForDoor.state != Door.DoorState.opening && waitForDoor.state != Door.DoorState.closing)
                    waitForDoor = null;
                return TaskState.running;
            } else if (pathIndex == -1 || navMeshPath.corners.Length == 0) {
                return TaskState.running;
            } else if (pathIndex <= navMeshPath.corners.Length - 1) {
                Vector3 inputVector = Vector3.zero;
                Vector3 nextPoint = navMeshPath.corners[pathIndex];
                float distance = Vector3.Distance(nextPoint, transform.position);
                if (distance > CORNER_ARRIVAL_DISTANCE) {
                    Vector3 direction = nextPoint - transform.position;
                    inputVector = direction;
                } else {
                    pathIndex += 1;
                }

                inputVector.y = 0;
                baseLookDirection = inputVector;
                input.moveDirection = speedCoefficient * inputVector.normalized;
                for (int i = 0; i < navMeshPath.corners.Length - 1; i++) {
                    Debug.DrawLine(navMeshPath.corners[i], navMeshPath.corners[i + 1], Color.white);
                }

                // check for doors between me and nextPoint
                if (waitForDoor == null) {
                    CheckForDoors(transform.position + new Vector3(0f, 1f, 0f), nextPoint + new Vector3(0f, 1f, 0f));
                }
                return TaskState.running;
            } else {
                return TaskState.success;
            }
        }

        public bool AtDestination() {
            return pathIndex == navMeshPath.corners.Length;
        }

        public void CheckForDoors(Vector3 position, Vector3 nextPoint) {
            Vector3 direction = nextPoint - position;
            Ray ray = new Ray(position, direction);
            RaycastHit[] hits = Physics.RaycastAll(ray, 2f, LayerUtil.GetLayerMask(Layer.interactive));
            Debug.DrawRay(position, direction, Color.green, 1f);
            foreach (RaycastHit hit in hits.OrderBy(h => h.distance)) {
                if (hit.transform.IsChildOf(transform.root))
                    continue;
                bool doorFound = hit.collider.CompareTag("door");
                Color color = doorFound ? Color.yellow : Color.red;
                Debug.DrawLine(position, hit.collider.bounds.center, color, 0.5f);
                if (doorFound) {
                    Door door = hit.collider.gameObject.GetComponent<Door>();
                    if (door.state != Door.DoorState.open) {
                        door.ActivateDoorknob(position, withKeySet: keyIds);
                        waitForDoor = door;
                    }
                }
            }
        }

        public void SetDestination() {
            NavMeshHit hit = new NavMeshHit();
            object keyObj = GetData(key);
            if (keyObj == null)
                return;
            Vector3 target = (Vector3)keyObj;
            NavMeshQueryFilter filter = new NavMeshQueryFilter {
                areaMask = LayerUtil.KeySetToNavLayerMask(keyIds)
            };
            if (NavMesh.SamplePosition(target, out hit, 10f, filter)) {
                Vector3 destination = hit.position;
                NavMesh.CalculatePath(transform.position, destination, filter, navMeshPath);
                pathIndex = 1;
            } else if (NavMesh.SamplePosition(target, out hit, 100f, filter)) {
                Vector3 destination = hit.position;
                NavMesh.CalculatePath(transform.position, destination, filter, navMeshPath);
                pathIndex = 1;
            } else {
                Debug.LogWarning($"could not find navmeshhit for {target}");
                navFailures += 1;
            }
        }
        public override void Reset() {
            base.Reset();
            SetDestination();
        }
    }

}