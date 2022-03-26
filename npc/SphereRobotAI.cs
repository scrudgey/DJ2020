using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
public class SphereRobotAI : IBinder<SightCone>, IDamageable, IListener {
    public SightCone sightCone;
    public NavMeshPath navMeshPath;
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

    private void OnDrawGizmos() {
        string customName = "Relic\\MaskedSpider.png";
        Gizmos.DrawIcon(lastSeenPlayerPosition, customName, true);
    }
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

        navMeshPath = new NavMeshPath();
    }
    void StartMoveRoutine() {
        ChangeState(new SphereMoveRoutine(this, patrolZone));
    }
    public void RoutineFinished(SphereControlState routine) {
        switch (routine) {
            default:
            case SearchDirectionState:
            case SphereAttackRoutine:
                ChangeState(new SphereMoveRoutine(this, patrolZone));
                break;
        }
    }
    private void ChangeState(SphereControlState routine) {
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

        for (int i = 0; i < navMeshPath.corners.Length - 1; i++) {
            Debug.DrawLine(navMeshPath.corners[i], navMeshPath.corners[i + 1], Color.white);
        }
    }
    void SetInputs() {
        PlayerInput input = stateMachine.getInput();
        sphereController.SetInputs(input);
        gunHandler.ProcessGunSwitch(input);
        gunHandler.SetInputs(input);
    }
    private void OnTriggerEnter(Collider other) {
        // Debug.Log(other);
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

            // TODO: change response depending on suspicion level
            // player suspicion, my awareness of player suspicion

            switch (stateMachine.currentState) {
                case SearchDirectionState:
                case SphereMoveRoutine:
                    ChangeState(new SphereAttackRoutine(this, gunHandler));
                    break;
            }

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

    public void TakeDamage<T>(T damage) where T : Damage {
        switch (stateMachine.currentState) {
            case SphereMoveRoutine:
                ChangeState(new SearchDirectionState(this, damage));
                break;
            case SearchDirectionState:
                // TODO: if ive been searching for a while, reset the search
                break;
        }
    }

    public void HearNoise(NoiseComponent noise) {
        // Debug.Log($"hearing noise {noise}");
        stateMachine.currentState.OnNoiseHeard(noise);
        switch (stateMachine.currentState) {
            case SphereMoveRoutine:
                ChangeState(new SearchDirectionState(this, noise));
                break;
            case SearchDirectionState:
                // TODO: if ive been searching for a while, reset the search
                break;
        }
    }
}
