using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KinematicCharacterController;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class WorkerNPCAI : IBinder<SightCone>, IListener, IHitstateSubscriber, IDamageReceiver {
    public enum WorkerType { sentry, visitActivities }
    WorkerType myType;
    public string dialogueName;
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

    [Header("character")]
    public Alertness alertness = Alertness.normal;
    public SpeechEtiquette[] etiquettes;
    // public Sprite portrait;

    [Header("specifics")]
    public Transform guardPoint;
    public Transform lookAtPoint;
    public List<WorkerLandmark> landmarkPointsOfInterest;
    public WorkerLandmark landmarkStation;
    public WorkerLandmark currentLandmark;
    public WorkerLandmark destinationLandmark;
    public bool notifyGuard;
    public bool guardNotified;
    public bool someoneWasShot;

    [HideInInspector]
    public NavMeshPath navMeshPath;
    WorkerNPCBrain stateMachine;
    Collider[] nearbyOthers;
    RaycastHit[] raycastHits;
    Vector3 closeness;
    float timeSinceInterrogatedStranger;
    public SpaceTimePoint lastSeenPlayerPosition;
    public SpaceTimePoint lastHeardPlayerPosition;
    public SpaceTimePoint lastHeardDisturbancePosition;
    List<Transform> otherTransforms = new List<Transform>();
    bool awareOfCorpse;
    Suspiciousness lastSuspicionLevel;
    float perceptionCountdown;
    bool panic;
    public void Awake() {
        nearbyOthers = new Collider[32];
        navMeshPath = new NavMeshPath();
        stateMachine = new WorkerNPCBrain();
        motor = GetComponent<KinematicCharacterMotor>();
        motor.SimulatedCharacterMass = UnityEngine.Random.Range(25f, 2500f);

        if (speechTextController == null) {
            GameObject obj = GameObject.Instantiate(Resources.Load("prefabs/speechOverlay")) as GameObject;
            SpeechTextController controller = obj.GetComponent<SpeechTextController>();
            this.speechTextController = controller;
            controller.followTransform = transform;
        }
        // if (highlight == null) {
        //     GameObject obj = GameObject.Instantiate(Resources.Load("prefabs/spottedHighlight")) as GameObject;
        //     SpottedHighlight spottedHighlight = obj.GetComponent<SpottedHighlight>();
        //     this.highlight = spottedHighlight;
        // }

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
        alertHandler.gameObject.SetActive(true);
    }
    public void OnPoolDectivate() {
        perceptionCountdown = 0f;
        lastSeenPlayerPosition = null;
        lastHeardDisturbancePosition = null;
        lastHeardPlayerPosition = null;

        lastSuspicionLevel = Suspiciousness.normal;
        alertHandler.gameObject.SetActive(false);// = true;
        speechTextController.enabled = true;
        stateMachine = new WorkerNPCBrain();

        EnterDefaultState();
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
        if (timeSinceInterrogatedStranger > 0f) {
            timeSinceInterrogatedStranger -= Time.deltaTime;
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
        switch (routine) {
            case WorkerPanicRunState:
            case WorkerReactToAttackState:
            case WorkerCowerState:
                panic = true;
                break;
            case WorkerInvestigateState:
                timeSinceInterrogatedStranger = 120f;
                break;
        }
    }
    void EnterDefaultState() {
        // TODO: enter panic
        if (panic) {
            if (UnityEngine.Random.Range(0f, 1f) < 0.5f) {
                ChangeState(new WorkerCowerState(this, characterController));
            } else {
                ChangeState(new WorkerPanicRunState(this, characterController));
            }
        }
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
            case WorkerNotifyGuardState:
                guardNotified = true;
                EnterDefaultState();
                break;
            case WorkerSearchDirectionState:
            case WorkerLoiterState:
                EnterDefaultState();
                break;
            case WorkerHeldAtGunpointState:
                Vector3 direction = GameManager.I.playerPosition - transform.position;
                Damage fakeDamage = new Damage(0f, direction, transform.position, GameManager.I.playerPosition);
                ChangeState(new WorkerReactToAttackState(this, speechTextController, fakeDamage, characterController));
                notifyGuard = true;
                break;
            case WorkerPanicRunState:
            case WorkerCowerState:
            case WorkerReactToAttackState:
                if (UnityEngine.Random.Range(0f, 1f) < 0.1f) {
                    panic = false;
                    EnterDefaultState();
                }
                if (UnityEngine.Random.Range(0f, 1f) < 0.5f) {
                    ChangeState(new WorkerCowerState(this, characterController));
                } else {
                    ChangeState(new WorkerPanicRunState(this, characterController));
                }
                break;
            case WorkerInvestigateState:
                timeSinceInterrogatedStranger = 120f;
                WorkerInvestigateState investigateState = (WorkerInvestigateState)routine;
                if (investigateState.dialogueResult == NeoDialogueMenu.DialogueResult.fail) {
                    ChangeState(new WorkerPanicRunState(this, characterController));
                    notifyGuard = true;
                } else if (investigateState.dialogueResult == NeoDialogueMenu.DialogueResult.stun) {
                    alertHandler.ShowWarn();
                    ChangeState(new WorkerPanicRunState(this, characterController));
                    notifyGuard = true;
                } else if (investigateState.gaveUp) {
                    alertHandler.ShowWarn();
                    ChangeState(new WorkerPanicRunState(this, characterController));
                    notifyGuard = true;
                } else goto default;
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
        if (hitState == HitState.dead || hitState == HitState.hitstun || hitState == HitState.zapped) {
            return;
        }
        if (noise == null || (noise.data.source != null && noise.data.source == transform.root.gameObject))
            return;

        if (stateMachine != null && stateMachine.currentState != null)
            stateMachine.currentState.OnNoiseHeard(noise);

        if (noise.data.isGunshot && noise.data.suspiciousness > Suspiciousness.suspicious) {
            lastHeardDisturbancePosition = new SpaceTimePoint(noise.transform.position);
            if (noise.data.player) {
                notifyGuard = true;
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
        } else {
            if (noise.data.suspiciousness > Suspiciousness.normal) {
                lastHeardDisturbancePosition = new SpaceTimePoint(noise.transform.position);
                if (noise.data.suspiciousness == Suspiciousness.suspicious) {
                    SuspicionRecord record = SuspicionRecord.noiseSuspicion();
                    GameManager.I.AddSuspicionRecord(record);
                    switch (stateMachine.currentState) {
                        case WorkerGuardState:
                        case WorkerLoiterState:
                            alertHandler.ShowWarn();
                            ChangeState(new WorkerSearchDirectionState(this, noise, characterController));
                            break;
                    }
                } else if (noise.data.suspiciousness == Suspiciousness.aggressive) {
                    SuspicionRecord record = SuspicionRecord.explosionSuspicion();
                    GameManager.I.AddSuspicionRecord(record);
                    notifyGuard = true;
                    switch (stateMachine.currentState) {
                        case WorkerGuardState:
                        case WorkerCowerState:
                        case WorkerPanicRunState:
                        case WorkerHeldAtGunpointState:
                        case WorkerLoiterState:
                            alertHandler.ShowAlert(useWarnMaterial: true);
                            Debug.Log($"worker react to attack state");
                            ChangeState(new WorkerReactToAttackState(this, speechTextController, noise, characterController));
                            break;
                    }
                }

            }
        }
    }

    void SightCheckPlayer() {
        Collider player = GameManager.I.playerCollider;
        if (Vector3.Dot(target.transform.up, player.bounds.center - transform.position) < 0) {
            Toolbox.AsyncClearLineOfSight(target.transform.position, player, (RaycastHit hit) => {
                if (hit.collider == player) Perceive(player, byPassVisibilityCheck: true);
            });
        }
    }
    public override void HandleValueChanged(SightCone t) {
        if (t.newestAddition != null) {
            Toolbox.AsyncClearLineOfSight(target.transform.position, t.newestAddition, (RaycastHit hit) => {
                if (hit.collider == t.newestAddition) Perceive(t.newestAddition);
            });
        }
    }

    void PerceiveFieldOfView() {
        foreach (Collider collider in target.fieldOfView) {
            if (collider == null)
                continue;
            Toolbox.AsyncClearLineOfSight(target.transform.position, collider, (RaycastHit hit) => {
                if (hit.collider == collider) Perceive(collider);
            });
        }
    }
    void Perceive(Collider other, bool byPassVisibilityCheck = false) {
        if (other == null) return;

        if (hitState == HitState.dead || hitState == HitState.hitstun || hitState == HitState.zapped) {
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
                if (bulletImpact.impacted.IsChildOf(transform)) {

                } else {
                    alertHandler.ShowAlert(useWarnMaterial: true);
                    someoneWasShot = true;
                    notifyGuard = true;
                    switch (stateMachine.currentState) {
                        case WorkerLoiterState:
                        case WorkerGuardState:
                        case WorkerInvestigateState:
                            ChangeState(new WorkerReactToAttackState(this, speechTextController, bulletImpact.damage, characterController));
                            break;
                    }
                }

            }
            SphereRobotAI guardAI = other.GetComponent<SphereRobotAI>();
            if (guardAI != null) {
                if (notifyGuard && !guardNotified) {
                    switch (stateMachine.currentState) {
                        case WorkerGuardState:
                        case WorkerLoiterState:
                        case WorkerPanicRunState:
                        case WorkerCowerState:
                            Debug.Log("changing state to notify guard");
                            ChangeState(new WorkerNotifyGuardState(this, characterController, speechTextController, guardAI));
                            break;
                    }
                }
            }
        }
        if (stateMachine.currentState != null)
            stateMachine.currentState.OnObjectPerceived(other);
    }

    void PerceivePlayerObject(Collider other, bool byPassVisibilityCheck = false) {
        if (GameManager.I.playerCharacterController.state == CharacterState.hvac || GameManager.I.playerCharacterController.state == CharacterState.hvacAim)
            return;
        float distance = Vector3.Distance(transform.position, other.bounds.center);
        if (byPassVisibilityCheck || GameManager.I.IsPlayerVisible(distance) || stateMachine.currentState is WorkerInvestigateState) {
            lastSeenPlayerPosition = new SpaceTimePoint(other.bounds.center);
            Suspiciousness playerTotalSuspicion = GameManager.I.GetTotalSuspicion();
            bool playerHasGunOut = GameManager.I.PlayerHasGunOut();
            // Reaction reaction = ReactToPlayerSuspicion();
            stateMachine.currentState.OnPlayerPerceived();
            if (playerHasGunOut) {
                switch (stateMachine.currentState) {
                    case WorkerGuardState:
                    case WorkerLoiterState:
                        alertHandler.ShowAlert(useWarnMaterial: true);
                        ChangeState(new WorkerHeldAtGunpointState(this, characterController, other.gameObject));
                        break;
                }
            } else if (playerTotalSuspicion == Suspiciousness.aggressive) {
                switch (stateMachine.currentState) {
                    case WorkerGuardState:
                    case WorkerLoiterState:
                        alertHandler.ShowAlert(useWarnMaterial: true);
                        ChangeState(new WorkerHeldAtGunpointState(this, characterController, other.gameObject));
                        break;
                    case WorkerHeldAtGunpointState:
                        Vector3 direction = GameManager.I.playerPosition - transform.position;
                        Damage fakeDamage = new Damage(0f, direction, transform.position, GameManager.I.playerPosition);
                        ChangeState(new WorkerReactToAttackState(this, speechTextController, fakeDamage, characterController));
                        break;
                }
            } else if (playerTotalSuspicion == Suspiciousness.suspicious) {
                switch (stateMachine.currentState) {
                    case WorkerGuardState:
                    case WorkerLoiterState:
                        alertHandler.ShowAlert(useWarnMaterial: true);
                        if (timeSinceInterrogatedStranger <= 0) {
                            ChangeState(new WorkerInvestigateState(this, characterController, speechTextController));
                        }
                        break;
                }
            }
        }
    }

    public DialogueCharacterInput MyCharacterInput() => new DialogueCharacterInput() {
        portrait = speechTextController.portrait,
        etiquettes = etiquettes,
        alertness = alertness,
        name = dialogueName
    };

    public Reaction ReactToPlayerSuspicion() {
        Suspiciousness totalSuspicion = GameManager.I.GetTotalSuspicion();
        SensitivityLevel sensitivityLevel = GameManager.I.GetCurrentSensitivity();
        lastSuspicionLevel = Toolbox.Max(lastSuspicionLevel, totalSuspicion);
        Reaction reaction = GameManager.I.GetSuspicionReaction(lastSuspicionLevel);
        Reaction unmodifiedReaction = GameManager.I.GetSuspicionReaction(lastSuspicionLevel, applyModifiers: false);
        return reaction;
    }

    public DamageResult TakeDamage(Damage damage) {
        // alertHandler.ShowAlert(useWarnMaterial: true);
        // TODO: change state
        // ChangeState(new CivilianReactToAttackState(this, speechTextController, damage, characterController));
        return DamageResult.NONE;
    }


#if UNITY_EDITOR
    void OnDrawGizmos() {
        if (stateMachine != null) {
            string labelText = $"state: {stateMachine.currentStateName}";
            Handles.Label(transform.position, labelText);

            // string customName = "Relic\\MaskedSpider.png";
            // Gizmos.DrawIcon(getLocationOfInterest(), customName, true);
        }
    }
#endif
}
