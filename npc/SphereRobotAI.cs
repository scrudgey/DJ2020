using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
public class SphereRobotAI : IBinder<SightCone> {
    public SightCone sightCone;
    public NavMeshAgent navMeshAgent;
    public SphereRobotController sphereController;
    public GunHandler gunHandler;
    private SphereRobotBrain stateMachine;
    float perceptionCountdown;
    public SphereCollider patrolZone;
    readonly float PERCEPTION_INTERVAL = 0.05f;
    readonly float MAXIMUM_SIGHT_RANGE = 50f;
    readonly float LOCK_ON_TIME = 0.5f;
    public Vector3 lastSeenPlayerPosition;
    public float timeSinceLastSeen;
    public Collider playerCollider;
    Vector3 previousPosition;
    void Start() {

        // TODO: save / load gun state
        gunHandler.primary = new GunInstance(Gun.Load("smg"));
        gunHandler.SwitchToGun(1);
        gunHandler.Reload();
        gunHandler.ClipIn();
        gunHandler.Rack();

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
        timeSinceLastSeen += Time.deltaTime;
        stateMachine.Update();
        SetInputs();
        perceptionCountdown -= Time.deltaTime;
        if (perceptionCountdown <= 0) {
            perceptionCountdown += PERCEPTION_INTERVAL;
            PerceiveFieldOfView();
        }
        if (timeSinceLastSeen < LOCK_ON_TIME && playerCollider != null) {
            if (TargetVisible(playerCollider))
                Perceive(playerCollider);
        }
    }
    void SetInputs() {
        PlayerInput input = stateMachine.getInput();
        sphereController.SetInputs(input);
        gunHandler.ProcessGunSwitch(input);
        gunHandler.SetInputs(input);
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
            if (collider == null)
                continue;
            if (TargetVisible(collider))
                Perceive(collider);
        }
    }
    void Perceive(Collider other) {
        stateMachine.currentState.OnObjectPerceived(other);
        if (other.transform.IsChildOf(GameManager.I.playerObject.transform)) {
            lastSeenPlayerPosition = other.bounds.center;
            timeSinceLastSeen = 0f;
            playerCollider = other;
        }
    }

    bool TargetVisible(Collider other) {
        Vector3 position = transform.position + new Vector3(0f, 1f, 0f);
        Vector3 direction = other.bounds.center - position;
        Ray ray = new Ray(position, direction);
        RaycastHit[] hits = Physics.RaycastAll(ray, MAXIMUM_SIGHT_RANGE, LayerUtil.GetMask(Layer.def, Layer.obj));
        foreach (RaycastHit hit in hits.OrderBy(h => h.distance)) {
            if (hit.transform.IsChildOf(transform.root))
                continue;
            TagSystemData tagData = Toolbox.GetTagData(hit.collider.gameObject);
            if (tagData.bulletPassthrough) continue;

            Color color = other == hit.collider ? Color.yellow : Color.red;
            Debug.DrawLine(position, hit.collider.bounds.center, color, 0.5f);
            return other == hit.collider;
        }
        return false;
    }
}
