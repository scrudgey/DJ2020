using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KinematicCharacterController;
using UnityEngine;
using UnityEngine.AI;

public class WorkerNPCAI : IBinder<SightCone>, IListener, IHitstateSubscriber, IDamageReceiver {
    public enum WorkerType { sentry, visitActivities }
    WorkerType myType;
    static readonly float PERCEPTION_INTERVAL = 0.025f;
    static readonly float avoidFactor = 1f;
    static readonly float avoidRadius = 0.5f;
    static readonly WaitForSeconds wait = new WaitForSeconds(1f);
    static readonly float MAXIMUM_SIGHT_RANGE = 25f;

    public Listener listener { get; set; }
    public HitState hitState { get; set; }
    public AlertHandler alertHandler;
    public SightCone sightCone;
    public CharacterController characterController;
    public GunHandler gunHandler;
    public KinematicCharacterMotor motor;
    public SpeechTextController speechTextController;

    [Header("specifics")]
    public Transform guardPoint;
    public Transform lookAtPoint;
    public List<WorkerLandmark> landmarkPointsOfInterest;
    public WorkerLandmark landmarkStation;
    public WorkerLandmark currentLandmark;

    [HideInInspector]
    public NavMeshPath navMeshPath;
    WorkerNPCBrain stateMachine;
    Collider[] nearbyOthers;
    RaycastHit[] raycastHits;
    Vector3 closeness;

    SpaceTimePoint lastSeenPlayerPosition;
    SpaceTimePoint lastHeardPlayerPosition;
    SpaceTimePoint lastHeardDisturbancePosition;
    List<Transform> otherTransforms = new List<Transform>();
    bool awareOfCorpse;
    Suspiciousness lastSuspicionLevel;
    float perceptionCountdown;
    public void Awake() {
        nearbyOthers = new Collider[32];
        navMeshPath = new NavMeshPath();
        stateMachine = new WorkerNPCBrain();
        motor = GetComponent<KinematicCharacterMotor>();
        motor.SimulatedCharacterMass = UnityEngine.Random.Range(25f, 2500f);

        Bind(sightCone.gameObject);
    }
    public void SetCurrentLandmark(WorkerLandmark landmark) {
        if (WorkerLandmark.visitors == null) {
            WorkerLandmark.visitors = new Dictionary<WorkerNPCAI, WorkerLandmark>();
        }
        WorkerLandmark.visitors[this] = landmark;

        currentLandmark = landmark;

    }
    public void ExcludeLandmark(WorkerLandmark landmark) {
        if (landmark.excludable) {
            landmark.excluded = true;
        }
    }
    public void UnexcludeCurrentLandmark() {
        if (currentLandmark != null && currentLandmark.excludable) {
            currentLandmark.excluded = false;
        }
    }
    void Start() {
        gunHandler.Holster();
        StartCoroutine(Toolbox.RunJobRepeatedly(findNearby));
    }
    public void OnPoolActivate() {
        Awake();
        listener = gameObject.GetComponentInChildren<Listener>();
    }
    void Update() {
        PlayerInput input = stateMachine.Update();
        input.preventWallPress = true;
        // avoid bunching with boids algorithm
        if (!input.CrouchDown && input.moveDirection != Vector3.zero) {
            Vector3 myPosition = transform.position;
            closeness = Vector3.zero;
            foreach (Transform otherTransform in otherTransforms) {
                Vector3 distance = (myPosition - otherTransform.position);
                distance = distance.normalized / distance.sqrMagnitude;
                closeness += distance;
            }
            closeness.y = 0;
            float magnitude = input.moveDirection.magnitude;
            input.moveDirection += avoidFactor * closeness;
            input.moveDirection = Vector3.ClampMagnitude(input.moveDirection, magnitude);
        }
        perceptionCountdown -= Time.deltaTime;
        if (perceptionCountdown <= 0) {
            perceptionCountdown += PERCEPTION_INTERVAL;
            PerceiveFieldOfView();
        }

        SetInputs(input);
        for (int i = 0; i < navMeshPath.corners.Length - 1; i++) {
            Debug.DrawLine(navMeshPath.corners[i], navMeshPath.corners[i + 1], Color.white);
        }
    }

    IEnumerator findNearby() {
        yield return new WaitForSeconds(UnityEngine.Random.Range(0f, 2f));
        int numColliders = Physics.OverlapSphereNonAlloc(transform.position, avoidRadius, nearbyOthers, LayerUtil.GetLayerMask(Layer.obj));
        closeness = Vector3.zero;
        otherTransforms = new List<Transform>();
        for (int i = 0; i < numColliders; i++) {
            Collider collider = nearbyOthers[i];
            if (collider == null || collider.gameObject == null || collider.transform.IsChildOf(transform))
                continue;
            // WorldNPCAI otherAI = collider.GetComponent<WorldNPCAI>();
            if (collider.CompareTag("actor")) {
                otherTransforms.Add(collider.transform);
            }
        }
    }

    public void Initialize(WorkerType workerType) {
        myType = workerType;
        EnterDefaultState();
    }
    public void ChangeState(WorkerNPCControlState routine) {
        stateMachine.ChangeState(routine);
    }
    void EnterDefaultState() {
        switch (myType) {
            default:
            case WorkerType.sentry:
                ChangeState(new WorkerGuardState(this, characterController, guardPoint.position, lookAtPoint.position));
                break;
            case WorkerType.visitActivities:
                ChangeState(new WorkerLoiterState(this, characterController, speechTextController));
                break;
        }
    }
    public void StateFinished(WorkerNPCControlState routine) {
        switch (routine) {
            case WorkerLoiterState:
                EnterDefaultState();
                break;
            case WorkerHeldAtGunpointState:
                Vector3 direction = GameManager.I.playerPosition - transform.position;
                Damage fakeDamage = new Damage(0f, direction, transform.position, GameManager.I.playerPosition);
                ChangeState(new WorkerReactToAttackState(this, speechTextController, fakeDamage, characterController));
                break;
            case WorkerPanicRunState:
            case WorkerCowerState:
            case WorkerReactToAttackState:
                if (UnityEngine.Random.Range(0f, 1f) < 0.1f) {
                    EnterDefaultState();
                }
                if (UnityEngine.Random.Range(0f, 1f) < 0.5f) {
                    ChangeState(new WorkerCowerState(this, characterController));
                } else {
                    ChangeState(new WorkerPanicRunState(this, characterController));
                }
                break;
            default:
                EnterDefaultState();
                break;
        }
    }


    void SetInputs(PlayerInput input) {
        characterController.SetInputs(input);
    }

    public void HearNoise(NoiseComponent noise) {
        if (noise == null || (noise.data.source != null && noise.data.source == transform.root.gameObject))
            return;
        if (noise.data.isGunshot && noise.data.suspiciousness > Suspiciousness.suspicious) {
            lastHeardDisturbancePosition = new SpaceTimePoint(noise.transform.position);
            if (noise.data.player) {
                lastHeardPlayerPosition = new SpaceTimePoint(noise.transform.position);
                SightCheckPlayer();
            }
            SuspicionRecord record = SuspicionRecord.gunshotsHeard();
            GameManager.I.AddSuspicionRecord(record);
            Ray ray = noise.data.ray;
            Vector3 rayDirection = ray.direction;
            Vector3 directionToNoise = transform.position - noise.transform.position;
            rayDirection.y = 0;
            directionToNoise.y = 0;
            float dotFactor = Vector3.Dot(rayDirection, directionToNoise);
            switch (stateMachine.currentState) {
                case WorkerLoiterState:
                case WorkerGuardState:
                case WorkerCowerState:
                case WorkerPanicRunState:
                case WorkerHeldAtGunpointState:
                    alertHandler.ShowAlert(useWarnMaterial: true);
                    ChangeState(new WorkerReactToAttackState(this, speechTextController, noise, characterController));
                    break;
            }
        } else { // not player
            if (noise.data.suspiciousness > Suspiciousness.normal) {
                lastHeardDisturbancePosition = new SpaceTimePoint(noise.transform.position);
                if (noise.data.suspiciousness == Suspiciousness.suspicious) {
                    SuspicionRecord record = SuspicionRecord.noiseSuspicion();
                    GameManager.I.AddSuspicionRecord(record);
                } else if (noise.data.suspiciousness == Suspiciousness.aggressive) {
                    SuspicionRecord record = SuspicionRecord.explosionSuspicion();
                    GameManager.I.AddSuspicionRecord(record);
                }
                switch (stateMachine.currentState) {
                    case WorkerGuardState:
                    case WorkerCowerState:
                    case WorkerPanicRunState:
                    case WorkerHeldAtGunpointState:
                    case WorkerLoiterState:
                        alertHandler.ShowAlert(useWarnMaterial: true);
                        ChangeState(new WorkerReactToAttackState(this, speechTextController, noise, characterController));
                        break;
                }
            }
        }
    }

    void SightCheckPlayer() {
        Collider player = GameManager.I.playerCollider;
        if (Vector3.Dot(target.transform.up, player.bounds.center - transform.position) < 0) {
            if (Toolbox.ClearLineOfSight(target.transform.position, player))
                Perceive(player, byPassVisibilityCheck: true);
        }
    }
    public override void HandleValueChanged(SightCone t) {
        if (t.newestAddition != null) {
            if (Toolbox.ClearLineOfSight(target.transform.position, t.newestAddition))
                Perceive(t.newestAddition);
        }
    }

    void PerceiveFieldOfView() {
        foreach (Collider collider in target.fieldOfView) {
            if (collider == null)
                continue;
            if (Toolbox.ClearLineOfSight(target.transform.position, collider))
                Perceive(collider);
        }
    }
    void Perceive(Collider other, bool byPassVisibilityCheck = false) {
        if (hitState == HitState.dead) {
            return;
        }
        if (other.transform.IsChildOf(GameManager.I.playerObject.transform)) {
            PerceivePlayerObject(other, byPassVisibilityCheck: byPassVisibilityCheck);
        } else {
            if (!awareOfCorpse) {
                // TODO: use tag instead
                Corpse corpse = other.GetComponent<Corpse>();
                if (corpse != null) {
                    awareOfCorpse = true;
                    // TODO: change state
                }
            }
            if (other.CompareTag("bulletImpact")) {
                BulletImpact bulletImpact = other.GetComponent<BulletImpact>();
                alertHandler.ShowAlert(useWarnMaterial: true);
                SuspicionRecord record = SuspicionRecord.shotSuspicion();
                GameManager.I.AddSuspicionRecord(record);
                // TODO: change states
            }
        }
    }

    void PerceivePlayerObject(Collider other, bool byPassVisibilityCheck = false) {
        float distance = Vector3.Distance(transform.position, other.bounds.center);
        if (byPassVisibilityCheck || GameManager.I.IsPlayerVisible(distance)) {
            lastSeenPlayerPosition = new SpaceTimePoint(other.bounds.center);
            Suspiciousness playerTotalSuspicion = GameManager.I.GetTotalSuspicion();
            Reaction reaction = ReactToPlayerSuspicion();
            stateMachine.currentState.OnPlayerPerceived();
            if (playerTotalSuspicion == Suspiciousness.suspicious) {
                switch (stateMachine.currentState) {
                    case WorkerGuardState:
                    case WorkerLoiterState:
                        alertHandler.ShowAlert(useWarnMaterial: true);
                        ChangeState(new WorkerHeldAtGunpointState(this, characterController, other.gameObject));
                        break;
                }
            } else if (playerTotalSuspicion == Suspiciousness.aggressive) {
                switch (stateMachine.currentState) {
                    case WorkerHeldAtGunpointState:
                        Vector3 direction = GameManager.I.playerPosition - transform.position;
                        Damage fakeDamage = new Damage(0f, direction, transform.position, GameManager.I.playerPosition);
                        ChangeState(new WorkerReactToAttackState(this, speechTextController, fakeDamage, characterController));
                        break;
                }
            }
        }
    }

    public Reaction ReactToPlayerSuspicion() {
        Suspiciousness totalSuspicion = GameManager.I.GetTotalSuspicion();
        SensitivityLevel sensitivityLevel = GameManager.I.GetCurrentSensitivity();
        lastSuspicionLevel = Toolbox.Max(lastSuspicionLevel, totalSuspicion);
        Reaction reaction = GameManager.I.GetSuspicionReaction(lastSuspicionLevel);
        Reaction unmodifiedReaction = GameManager.I.GetSuspicionReaction(lastSuspicionLevel, applyModifiers: false);
        return reaction;
    }

    public DamageResult TakeDamage(Damage damage) {
        alertHandler.ShowAlert(useWarnMaterial: true);
        // TODO: change state
        // ChangeState(new CivilianReactToAttackState(this, speechTextController, damage, characterController));
        return DamageResult.NONE;
    }
}
