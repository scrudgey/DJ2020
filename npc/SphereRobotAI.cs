using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
public class SphereRobotAI : IBinder<SightCone> {
    public SightCone sightCone;
    public NavMeshAgent navMeshAgent;
    public SphereRobotController sphereController;
    private SphereRobotBrain stateMachine;
    float perceptionCountdown;
    public SphereCollider patrolZone;
    readonly float PERCEPTION_INTERVAL = 0.05f;
    readonly float MAXIMUM_SIGHT_RANGE = 10f;
    public Vector3 lastSeenPlayerPosition;
    Vector3 previousPosition;
    void Start() {
        Bind(sightCone.gameObject);
        stateMachine = new SphereRobotBrain();
        StartMoveRoutine();
        // navMeshAgent.c
        // navMeshAgent.updatePosition = false;
        navMeshAgent.updateRotation = false;
        navMeshAgent.updateUpAxis = false;
    }
    void StartMoveRoutine() {
        ChangeState(new SphereMoveRoutine(this, navMeshAgent, patrolZone));
    }
    public void ChangeState(SphereControlState routine) {
        stateMachine.ChangeState(routine);
    }
    void Update() {
        stateMachine.Update();
        SetInputs();
        perceptionCountdown -= Time.deltaTime;
        if (perceptionCountdown <= 0) {
            perceptionCountdown += PERCEPTION_INTERVAL;
            PerceiveFieldOfView();
        }
    }
    void SetInputs() {
        sphereController.SetInputs(stateMachine.getInput());
    }
    private void OnTriggerEnter(Collider other) {
        Debug.Log(other);
    }
    public override void HandleValueChanged(SightCone t) {
        if (t.newestAddition != null) {
            if (TargetVisible(t.newestAddition))
                Perceive(t.newestAddition);
        }
    }
    void PerceiveFieldOfView() {
        foreach (Collider collider in target.fieldOfView) {
            if (TargetVisible(collider))
                Perceive(collider);
        }
    }
    void Perceive(Collider other) {
        stateMachine.currentState.OnObjectPerceived(other);
        if (other.transform.IsChildOf(GameManager.I.playerObject.transform)) {
            lastSeenPlayerPosition = other.bounds.center;
        }
        // CharacterController characterController = other.GetComponentInChildren<CharacterController>();
        // // // Debug.DrawLine(transform.position, other.transform.position, Color.yellow, 1f);
        // if (characterController != null) {
        //     Debug.Log($"Perceiving character {other}");
        //     if (characterController.gameObject == GameManager.I.playerObject) {

        //     }
        // }
    }
    void LateUpdate() {
        // navMeshAgent.updatePosition = transform.position;
        // navMeshAgent.
        // Vector3 delta = transform.position - previousPosition;
        // navMeshAgent.Move(delta);
        // previousPosition = transform.position;
    }

    bool TargetVisible(Collider other) {
        Vector3 position = transform.position + new Vector3(0f, 1f, 0f);
        Vector3 direction = other.bounds.center - position;
        Ray ray = new Ray(position, direction);
        RaycastHit[] hits = Physics.RaycastAll(ray, MAXIMUM_SIGHT_RANGE, LayerUtil.GetMask(Layer.def, Layer.obj));
        foreach (RaycastHit hit in hits.OrderBy(h => h.distance)) {
            if (hit.transform.IsChildOf(transform.root))
                continue;
            Color color = other == hit.collider ? Color.yellow : Color.red;
            Debug.DrawLine(position, hit.collider.bounds.center, color, 0.5f);
            return other == hit.collider;
        }
        return false;
    }
}
