using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
public class SphereRobotAI : IBinder<SightCone>, IDamageable, IListener {
    public SightCone sightCone;
    public NavMeshPath navMeshPath; // TODO: remove this
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
    public Alertness alertness;
    public bool recentlyInCombat;
    public bool recentHeardSuspicious;
    public bool recentlySawSuspicious;
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
        ChangeState(new SphereMoveRoutine(this, patrolZone));

        navMeshPath = new NavMeshPath();
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
        PlayerInput input = stateMachine.Update();
        SetInputs(input);
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
    void SetInputs(PlayerInput input) {
        sphereController.SetInputs(input);
        gunHandler.ProcessGunSwitch(input);
        gunHandler.SetInputs(input);
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
        if (other.transform.IsChildOf(GameManager.I.playerObject.transform)) {
            PerceivePlayerObject(other);
        } else {
            stateMachine.currentState.OnObjectPerceived(other);
        }
    }

    void PerceivePlayerObject(Collider other) {
        float distance = Vector3.Distance(transform.position, other.bounds.center);
        // LightLevelProbe
        if (GameManager.I.IsPlayerVisible(distance)) {
            stateMachine.currentState.OnObjectPerceived(other);
            lastSeenPlayerPosition = other.bounds.center;
            timeSinceLastSeen = 0f;
            playerCollider = other;

            ReactToPlayerSuspicion();

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
            // Debug.DrawLine(position, hit.collider.bounds.center, color, 0.5f);
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


    public void ReactToPlayerSuspicion() {
        // TODO: change response depending on suspicion level
        // player suspicion, my awareness of player suspicion

        // levels have sensitivity property
        if (GameManager.I.gameData.levelData.sensitivityLevel == SensitivityLevel.publicProperty) {
            // guard AI: focus
            alertness;

            //player character: activity
            GameManager.I.playerInteractor.GetSuspiciousness();

            //player character: appearance
            GameManager.I.PlayerAppearance();

            //AI: state of knowledge
            recentHeardSuspicious;
            recentlySawSuspicious;
            recentlyInCombat;

        } else if (GameManager.I.gameData.levelData.sensitivityLevel == SensitivityLevel.semiprivateProperty) {

        } else if (GameManager.I.gameData.levelData.sensitivityLevel == SensitivityLevel.privateProperty) {

        } else if (GameManager.I.gameData.levelData.sensitivityLevel == SensitivityLevel.restrictedProperty) {

        }
    }
}
