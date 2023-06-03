using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Easings;
using KinematicCharacterController;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
public enum Reaction { ignore, attack, investigate }

public class SphereRobotAI : IBinder<SightCone>, IDamageReceiver, IListener, IHitstateSubscriber, IPoolable {
    public Destructible characterHurtable;
    public Listener listener { get; set; }
    public HitState hitState { get; set; }
    public SightCone sightCone;
    public Transform sightOrigin;
    public NavMeshPath navMeshPath; // TODO: remove this
    public KinematicCharacterMotor motor;
    public CharacterController characterController;
    public GunHandler gunHandler;
    public AlertHandler alertHandler;
    public SphereRobotBrain stateMachine;
    public SpeechTextController speechTextController;
    float perceptionCountdown;
    public SphereCollider patrolZone;
    readonly float PERCEPTION_INTERVAL = 0.025f;
    readonly float MAXIMUM_SIGHT_RANGE = 25f;
    readonly float LOCK_ON_TIME = 0.5f;
    float timeSinceInvestigatedFootsteps;
    float timeSinceInterrogatedStranger;
    public float timeSinceLastSeen;
    public Collider playerCollider;
    public Alertness alertness = Alertness.normal;
    public bool recentlyInCombat;
    public Suspiciousness recentHeardSuspicious;
    public Suspiciousness recentlySawSuspicious;
    public PatrolRoute patrolRoute;
    public bool skipShootAnimation;
    NoiseComponent lastGunshotHeard;
    public SpaceTimePoint lastSeenPlayerPosition;
    public SpaceTimePoint lastHeardPlayerPosition;
    public SpaceTimePoint lastHeardDisturbancePosition;
    Damage lastDamage;
    // public bool overrideDefaultState;
    private float footstepImpulse;
    public SpottedHighlight highlight;
    public SpeechEtiquette[] etiquettes;
    public Sprite portrait;
    bool awareOfCorpse;
    Collider[] nearbyOthers;
    RaycastHit[] raycastHits;
    public HashSet<int> physicalKeys;
    Vector3 closeness;
    List<Transform> otherTransforms = new List<Transform>();
    static readonly float avoidFactor = 5f;
    static readonly float avoidRadius = 0.2f;
    public void Awake() {
        raycastHits = new RaycastHit[1];
        nearbyOthers = new Collider[32];
        // sphereController = controllable.GetComponent<CharacterController>();
        alertHandler.Hide();
        Bind(sightCone.gameObject);
        navMeshPath = new NavMeshPath();
        stateMachine = new SphereRobotBrain();
        motor = GetComponent<KinematicCharacterMotor>();
        if (characterHurtable != null) {
            characterHurtable.OnHitStateChanged += ((IHitstateSubscriber)this).HandleHurtableChanged;
        }
    }
    void Start() {
        StartCoroutine(Toolbox.RunJobRepeatedly(findNearby));
    }
    public void Initialize() {
        EnterDefaultState();
    }
    override public void OnDestroy() {
        base.OnDestroy();
        characterHurtable.OnHitStateChanged -= ((IHitstateSubscriber)this).HandleHurtableChanged;
    }

    void EnterDefaultState() {
        if (patrolRoute != null) {
            ChangeState(new SpherePatrolState(this, patrolRoute, characterController));
        } else {
            // ChangeState(new SphereMoveState(this, patrolZone));
        }
    }

    public Vector3 getLocationOfInterest() {
        if (lastSeenPlayerPosition != null && lastHeardPlayerPosition != null) {
            if (lastSeenPlayerPosition.time > lastHeardPlayerPosition.time) {
                return lastSeenPlayerPosition.position;
            } else return lastHeardPlayerPosition.position;
        } else if (lastSeenPlayerPosition != null) {
            return lastSeenPlayerPosition.position;
        } else if (lastHeardPlayerPosition != null) {
            return lastHeardPlayerPosition.position;
        } else if (lastHeardDisturbancePosition != null) {
            return lastHeardDisturbancePosition.position;
        } else return Vector3.zero;
    }

    public void StateFinished(SphereControlState routine) {
        switch (routine) {
            default:
                EnterDefaultState();
                break;
            case ReportGunshotsState:
                ChangeState(new SearchDirectionState(this, lastGunshotHeard, characterController, doIntro: false, speedCoefficient: 2f));
                break;
            case SphereHoldAtGunpointState:
                SphereHoldAtGunpointState holdAtGunpointState = (SphereHoldAtGunpointState)routine;
                if (holdAtGunpointState.isPlayerSuspicious()) {
                    ChangeState(new SphereAttackState(this, gunHandler, characterController));
                } else ChangeState(new SphereInvestigateState(this, highlight, characterController));
                break;
            case SphereInvestigateState:
                Debug.Log("leaving investigate state");
                timeSinceInterrogatedStranger = 120f;
                highlight.target = null;
                SphereInvestigateState investigateState = (SphereInvestigateState)routine;
                if (investigateState.dialogueResult == DialogueController.DialogueResult.fail) {
                    ChangeState(new SphereHoldAtGunpointState(this));
                } else if (investigateState.dialogueResult == DialogueController.DialogueResult.stun) {
                    alertHandler.ShowWarn();
                    ChangeState(new StunState(this));
                } else if (investigateState.isPlayerAggressive()) {
                    alertHandler.ShowWarn();
                    ChangeState(new SphereHoldAtGunpointState(this));
                } else if (investigateState.isPlayerSuspicious()) {
                    alertHandler.ShowWarn();
                    ChangeState(new SphereHoldAtGunpointState(this));
                } else goto default;
                break;
            case PauseState:
                PauseState pauser = (PauseState)routine;
                ChangeState(pauser.nextState);
                break;
            case ReactToAttackState:
                // TODO: do something different. we were just attacked
                if (lastDamage != null) {
                    ChangeState(new SearchDirectionState(this, lastDamage, characterController, doIntro: false, speedCoefficient: 2f));
                } else if (getLocationOfInterest() != Vector3.zero) {
                    ChangeState(new SearchDirectionState(this, getLocationOfInterest(), characterController, doIntro: false, speedCoefficient: 2f));
                } else {
                    EnterDefaultState();
                }
                break;
            case StopAndListenState:
                StopAndListenState listenState = (StopAndListenState)routine;
                SphereControlState nextState = listenState.getNextState();
                ChangeState(nextState);
                break;
            case SearchDirectionState:
                alertHandler.ShowGiveUp();
                EnterDefaultState();
                break;
            case SphereAttackState:
                if (!GameManager.I.gameData.levelState.anyAlarmActive()) {
                    if (lastDamage != null) {
                        ChangeState(new ReportToHQState(this, speechTextController, lastDamage));
                    } else if (getLocationOfInterest() != Vector3.zero) {
                        ChangeState(new ReportToHQState(this, speechTextController, getLocationOfInterest()));
                    } else {
                        EnterDefaultState();
                    }
                } else {
                    EnterDefaultState();
                }
                break;
            case ReportToHQState:
                if (lastDamage != null) {
                    ChangeState(new SearchDirectionState(this, lastDamage, characterController, doIntro: false));
                } else if (getLocationOfInterest() != Vector3.zero) {
                    ChangeState(new SearchDirectionState(this, getLocationOfInterest(), characterController, doIntro: false));
                } else {
                    EnterDefaultState();
                }
                break;
            case DisableAlarmState:
                EnterDefaultState();
                break;
        }
    }
    public void ChangeState(SphereControlState routine) {
        stateMachine.ChangeState(routine);
        switch (routine) {
            case SphereInvestigateState:
                timeSinceInterrogatedStranger = 120f;
                break;
            case StopAndListenState:
                timeSinceInvestigatedFootsteps = 10f;
                break;
            case SearchDirectionState:
                // listener.SetListenRadius(radius: 2f);
                break;
            case ReportToHQState:
            case ReactToAttackState:
            case SphereAttackState attack:
            case InvestigateCorpseState:
                awareOfCorpse = true;
                recentlyInCombat = true;
                break;
        }
    }
    IEnumerator findNearby() {
        yield return new WaitForSeconds(UnityEngine.Random.Range(0.1f, 0.5f));
        int numColliders = Physics.OverlapSphereNonAlloc(transform.position, avoidRadius, nearbyOthers, LayerUtil.GetLayerMask(Layer.obj));
        closeness = Vector3.zero;
        otherTransforms = new List<Transform>();
        for (int i = 0; i < numColliders; i++) {
            Collider collider = nearbyOthers[i];
            if (collider == null || collider.gameObject == null || collider.transform.IsChildOf(transform))
                continue;
            SphereRobotAI otherAI = collider.GetComponent<SphereRobotAI>();
            if (otherAI != null) {
                otherTransforms.Add(otherAI.transform);
            }
        }
    }
    void Update() {
        if (hitState == HitState.dead) {
            return;
        }
        timeSinceLastSeen += Time.deltaTime;
        PlayerInput input = stateMachine.Update();
        input.preventWallPress = true;

        // avoid bunching with boids algorithm
        // if (!input.CrouchDown) {
        //     // float avoidFactor = 0.1f;
        //     // float avoidRadius = 2f;

        //     // float avoidFactor = 1f;
        //     // float avoidRadius = 0.5f;

        //     float avoidFactor = 5f;
        //     float avoidRadius = 0.2f;

        //     int numColliders = Physics.OverlapSphereNonAlloc(transform.position, avoidRadius, nearbyOthers, LayerUtil.GetLayerMask(Layer.obj));
        //     Vector3 closeness = Vector3.zero;
        //     for (int i = 0; i < numColliders; i++) {
        //         Collider collider = nearbyOthers[i];
        //         if (collider == null || collider.gameObject == null)
        //             continue;
        //         if (collider.transform.IsChildOf(transform))
        //             continue;
        //         SphereRobotAI otherAI = collider.GetComponent<SphereRobotAI>();
        //         if (otherAI != null) {
        //             closeness += transform.position - otherAI.transform.position;
        //         }
        //     }
        //     closeness.y = 0;
        //     input.moveDirection += avoidFactor * closeness;
        // }
        if (!input.CrouchDown) {
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


        SetInputs(input);
        perceptionCountdown -= Time.deltaTime;
        if (perceptionCountdown <= 0) {
            perceptionCountdown += PERCEPTION_INTERVAL;
            PerceiveFieldOfView();
        }
        if (timeSinceLastSeen < LOCK_ON_TIME && playerCollider != null) {
            SightCheckPlayer();
        }
        if (footstepImpulse > 0f) {
            footstepImpulse -= Time.deltaTime;
        }
        if (timeSinceInvestigatedFootsteps > 0f) {
            timeSinceInvestigatedFootsteps -= Time.deltaTime;
        }
        if (timeSinceInterrogatedStranger > 0f) {
            timeSinceInterrogatedStranger -= Time.deltaTime;
        }
        for (int i = 0; i < navMeshPath.corners.Length - 1; i++) {
            Debug.DrawLine(navMeshPath.corners[i], navMeshPath.corners[i + 1], Color.white);
        }
    }

    void SightCheckPlayer() {
        Collider player = GameManager.I.playerCollider;
        if (Vector3.Dot(target.transform.up, player.bounds.center - transform.position) < 0) {
            if (ClearLineOfSight(player))
                Perceive(player, byPassVisibilityCheck: true);
        }
    }
    void SetInputs(PlayerInput input) {
        characterController.SetInputs(input);
    }
    public override void HandleValueChanged(SightCone t) {
        if (t.newestAddition != null) {
            if (ClearLineOfSight(t.newestAddition))
                Perceive(t.newestAddition);
        }
    }
    void PerceiveFieldOfView() {
        foreach (Collider collider in target.fieldOfView) {
            if (collider == null)
                continue;
            if (ClearLineOfSight(collider))
                Perceive(collider);
        }
    }
    void Perceive(Collider other, bool byPassVisibilityCheck = false) {
        if (other.transform.IsChildOf(GameManager.I.playerObject.transform)) {
            PerceivePlayerObject(other, byPassVisibilityCheck: byPassVisibilityCheck);
        } else {
            if (!awareOfCorpse) {
                // TODO: use tag instead
                Corpse corpse = other.GetComponent<Corpse>();
                if (corpse != null) {
                    awareOfCorpse = true;
                    if (!corpse.reported) {
                        alertHandler.ShowWarn();
                        switch (stateMachine.currentState) {
                            case SearchDirectionState:
                            case SphereMoveState:
                            case SpherePatrolState:
                            case FollowTheLeaderState:
                            case StopAndListenState:
                            case SphereInvestigateState:
                                corpse.reported = true;
                                ChangeState(new InvestigateCorpseState(this, corpse, speechTextController, characterController));
                                break;
                        }
                    }
                }
            }
            if (other.CompareTag("bulletImpact")) {
                BulletImpact bulletImpact = other.GetComponent<BulletImpact>();
                switch (stateMachine.currentState) {
                    case SphereMoveState:
                    case SpherePatrolState:
                    case FollowTheLeaderState:
                    case PauseState:
                    case DisableAlarmState:
                    case InvestigateCorpseState:
                    case SphereInvestigateState:
                    case SphereHoldAtGunpointState:
                    case StopAndListenState:
                        alertHandler.ShowAlert(useWarnMaterial: true);
                        SuspicionRecord record = SuspicionRecord.shotSuspicion();
                        GameManager.I.AddSuspicionRecord(record);
                        ChangeState(new ReactToAttackState(this, speechTextController, bulletImpact.damage, characterController, initialPause: 0f));
                        break;
                }
            }
            if (other.CompareTag("tamperEvidence")) {
                TamperEvidence evidence = other.GetComponent<TamperEvidence>();
                if (evidence.suspicious && !evidence.reported) {
                    switch (stateMachine.currentState) {
                        case SphereMoveState:
                        case SpherePatrolState:
                        case FollowTheLeaderState:
                        case PauseState:
                        case StopAndListenState:
                            evidence.reported = true;
                            alertHandler.ShowAlert(useWarnMaterial: true);
                            ChangeState(new ReactToTamperState(this, evidence, speechTextController, characterController));
                            break;
                    }
                }
            }
            
            if (stateMachine.currentState != null)
                stateMachine.currentState.OnObjectPerceived(other);
        }
    }

    void PerceivePlayerObject(Collider other, bool byPassVisibilityCheck = false) {
        float distance = Vector3.Distance(transform.position, other.bounds.center);
        if (byPassVisibilityCheck || GameManager.I.IsPlayerVisible(distance) || stateMachine.currentState is SphereInvestigateState) {
            stateMachine.currentState.OnObjectPerceived(other);
            lastSeenPlayerPosition = new SpaceTimePoint(other.bounds.center);
            timeSinceLastSeen = 0f;
            playerCollider = other;
            Reaction reaction = ReactToPlayerSuspicion();
            if (reaction == Reaction.attack) { // TODO: investigate routine
                switch (stateMachine.currentState) {
                    case SearchDirectionState:
                    case SphereMoveState:
                    case SpherePatrolState:
                    case ReactToAttackState:
                    case FollowTheLeaderState:
                    case StopAndListenState:
                    case SphereInvestigateState:
                        alertHandler.ShowAlert();
                        ChangeState(new SphereAttackState(this, gunHandler, characterController));
                        break;
                }
            } else if (reaction == Reaction.investigate) {
                switch (stateMachine.currentState) {
                    case SearchDirectionState:
                    case SphereMoveState:
                    case SpherePatrolState:
                    case ReactToAttackState:
                    case FollowTheLeaderState:
                    case StopAndListenState:
                        if (timeSinceInterrogatedStranger <= 0) {
                            alertHandler.ShowWarn();
                            GameManager.I.StartSpottedCutscene(gameObject);
                            ChangeState(new PauseState(this, new SphereInvestigateState(this, highlight, characterController), 1f));
                        }
                        break;
                }
            } else if (reaction == Reaction.ignore) {
                switch (stateMachine.currentState) {
                    case SearchDirectionState:
                        alertHandler.ShowGiveUp();
                        ChangeState(new PauseState(this, new SpherePatrolState(this, patrolRoute, characterController), 1f));
                        break;
                    case StopAndListenState:
                        alertHandler.ShowGiveUp();
                        StopAndListenState listenState = (StopAndListenState)stateMachine.currentState;
                        ChangeState(listenState.getNextState());
                        break;
                }
            }
        }
    }

    bool ClearLineOfSight(Collider other) {
        Vector3 position = sightOrigin.position; // TODO: configurable

        // Vector3[] directions = new Vector3[]{
        //     other.bounds.center - position,
        //     (other.bounds.center + other.bounds.extents) - position,
        //     (other.bounds.center - other.bounds.extents) - position,
        // };
        // float distance = Vector3.Distance(other.bounds.center, transform.position);

        // Physics.ClosestPoint can only be used with a BoxCollider, SphereCollider, CapsuleCollider and a convex MeshCollider.
        Vector3[] directions = new Vector3[0];
        float distance = 0;
        if (other is BoxCollider || other is SphereCollider || other is CapsuleCollider) {
            directions = new Vector3[]{
                other.ClosestPoint(position) - position
            };
        } else {
            directions = new Vector3[]{
                other.bounds.center - position
            };
        }
        bool clearLineOfSight = false;
        foreach (Vector3 direction in directions) {
            // distance = Math.Min(direction.magnitude, MAXIMUM_SIGHT_RANGE);
            distance = direction.magnitude;
            if (distance > MAXIMUM_SIGHT_RANGE)
                return false;
            Ray ray = new Ray(position, direction);
            int numberHits = Physics.RaycastNonAlloc(ray, raycastHits, distance * 0.99f, LayerUtil.GetLayerMask(Layer.def, Layer.obj, Layer.interactive), QueryTriggerInteraction.Ignore);
            clearLineOfSight |= numberHits == 0;
        }
        return clearLineOfSight;
    }

    public DamageResult TakeDamage(Damage damage) {
        lastDamage = damage;
        if (stateMachine != null && stateMachine.currentState != null)
            stateMachine.currentState.OnDamage(damage);
        switch (stateMachine.currentState) {
            case SphereMoveState:
            case SpherePatrolState:
            case SearchDirectionState:
            case FollowTheLeaderState:
            case DisableAlarmState:
            case ReportToHQState:
            case StopAndListenState:
                // TODO: better logic here?
                alertHandler.ShowAlert(useWarnMaterial: true);
                // if (GameManager.I.gameData.levelState.anyAlarmActive()) {
                //     ChangeState(new SearchDirectionState(this, damage, doIntro: false, speedCoefficient: ));
                // } else {
                ChangeState(new ReactToAttackState(this, speechTextController, damage, characterController));
                // }
                break;
        }
        return DamageResult.NONE;
    }

    public void HearNoise(NoiseComponent noise) {
        if (noise == null)
            return;
        if (noise.data.source != null && noise.data.source == transform.root.gameObject)
            return;
        recentHeardSuspicious = Toolbox.Max<Suspiciousness>(recentHeardSuspicious, noise.data.suspiciousness);

        if (stateMachine != null && stateMachine.currentState != null)
            stateMachine.currentState.OnNoiseHeard(noise);

        if (noise.data.isGunshot && noise.data.suspiciousness > Suspiciousness.suspicious) {
            lastHeardDisturbancePosition = new SpaceTimePoint(noise.transform.position);

            if (noise.data.player) {
                lastHeardPlayerPosition = new SpaceTimePoint(noise.transform.position);
                SightCheckPlayer();
            }
            // else {
            //     lastHeardDisturbancePosition = new SpaceTimePoint(noise.transform.position);
            // }
            lastGunshotHeard = noise;
            SuspicionRecord record = SuspicionRecord.gunshotsHeard();
            GameManager.I.AddSuspicionRecord(record);

            Ray ray = noise.data.ray;
            Vector3 rayDirection = ray.direction;
            Vector3 directionToNoise = transform.position - noise.transform.position;
            rayDirection.y = 0;
            directionToNoise.y = 0;
            float dotFactor = Vector3.Dot(rayDirection, directionToNoise);
            // Debug.Log($"hear gunshot: {stateMachine.currentState}");
            switch (stateMachine.currentState) {
                case ReportToHQState:
                case ReactToTamperState:
                case StunState:
                case SphereMoveState:
                case SpherePatrolState:
                case FollowTheLeaderState:
                case PauseState:
                case DisableAlarmState:
                case InvestigateCorpseState:
                case SphereInvestigateState:
                case SphereHoldAtGunpointState:
                case StopAndListenState:
                    alertHandler.ShowAlert(useWarnMaterial: true);
                    if (GameManager.I.gameData.levelState.anyAlarmActive()) {
                        // alarm is already active; no need to handle reporting to HQ. run to firefight.
                        ChangeState(new SearchDirectionState(this, noise, characterController, doIntro: false, speedCoefficient: 2f));
                    } else {
                        if (dotFactor < 1f) {
                            // gunshots not directed at me; report to HQ and then run to battle
                            if (GameManager.I.levelHQTerminal() != null && !GameManager.I.isAlarmRadioInProgress(gameObject)) {
                                ChangeState(new ReportGunshotsState(this, speechTextController, noise));
                            } else {
                                ChangeState(new SearchDirectionState(this, noise, characterController, doIntro: false, speedCoefficient: 2f));
                            }
                        } else {
                            // gunshots directed at me
                            ChangeState(new ReactToAttackState(this, speechTextController, noise, characterController));
                        }
                    }
                    break;
                case SearchDirectionState:
                    ChangeState(new SearchDirectionState(this, noise, characterController, doIntro: false, speedCoefficient: 1f));
                    break;
            }
        } else if (noise.data.player) {
            lastHeardPlayerPosition = new SpaceTimePoint(noise.transform.position);
            if (noise.data.suspiciousness > Suspiciousness.normal) {
                SuspicionRecord record = SuspicionRecord.noiseSuspicion();
                GameManager.I.AddSuspicionRecord(record);
                switch (stateMachine.currentState) {
                    case SphereMoveState:
                    case SpherePatrolState:
                    case FollowTheLeaderState:
                    case DisableAlarmState:
                        alertHandler.ShowWarn();
                        ChangeState(new SearchDirectionState(this, noise, characterController));
                        break;
                    case SearchDirectionState:
                        // if (stateMachine.timeInCurrentState > 3f)
                        ChangeState(new SearchDirectionState(this, noise, characterController, doIntro: false));
                        break;
                }
            } else if (noise.data.isFootsteps) {
                HandleFootstepNoise(noise);
            }
        } else {
            // not player
            if (noise.data.suspiciousness > Suspiciousness.normal) {
                lastHeardDisturbancePosition = new SpaceTimePoint(noise.transform.position);
                if (noise.data.suspiciousness == Suspiciousness.suspicious) {
                    SuspicionRecord record = SuspicionRecord.noiseSuspicion();
                    GameManager.I.AddSuspicionRecord(record);

                    switch (stateMachine.currentState) {
                        case SphereMoveState:
                        case SpherePatrolState:
                        case FollowTheLeaderState:
                        case DisableAlarmState:
                            alertHandler.ShowWarn();
                            ChangeState(new SearchDirectionState(this, noise, characterController));
                            break;
                    }
                } else if (noise.data.suspiciousness == Suspiciousness.aggressive) {
                    SuspicionRecord record = SuspicionRecord.explosionSuspicion();
                    GameManager.I.AddSuspicionRecord(record);
                    switch (stateMachine.currentState) {
                        case SphereMoveState:
                        case SpherePatrolState:
                        case FollowTheLeaderState:
                        case DisableAlarmState:
                            alertHandler.ShowWarn();
                            ChangeState(new ReportToHQState(this, speechTextController, noise));
                            break;
                    }
                }
            }
        }
    }

    void HandleFootstepNoise(NoiseComponent noise) {
        footstepImpulse += noise.data.volume * 2f;
        bool reachedFootstepThreshold = footstepImpulse > 6f;
        bool notBoredOfFootsteps = timeSinceInvestigatedFootsteps <= 0 && timeSinceInterrogatedStranger <= 0;
        if (GameManager.I.gameData.levelState.template.sensitivityLevel == SensitivityLevel.publicProperty) {
            if (reachedFootstepThreshold && recentlyInCombat && notBoredOfFootsteps) {
                switch (stateMachine.currentState) {
                    case SphereMoveState:
                    case SpherePatrolState:
                    case FollowTheLeaderState:
                        // timeSinceInvestigatedFootsteps = 10f;
                        alertHandler.ShowWarn();
                        ChangeState(new StopAndListenState(this, stateMachine.currentState, speechTextController, characterController));
                        break;
                }
            }
        } else if (GameManager.I.gameData.levelState.template.sensitivityLevel >= SensitivityLevel.privateProperty) {
            if (reachedFootstepThreshold && notBoredOfFootsteps)
                switch (stateMachine.currentState) {
                    case SphereMoveState:
                    case SpherePatrolState:
                    case FollowTheLeaderState:
                        // timeSinceInvestigatedFootsteps = 10f;
                        alertHandler.ShowWarn();
                        ChangeState(new StopAndListenState(this, stateMachine.currentState, speechTextController, characterController));
                        break;
                }
        }
    }

    public Reaction ReactToPlayerSuspicion() {
        // guard AI: focus
        // alertness;
        // AI: state of knowledge
        // recentHeardSuspicious;
        // recentlySawSuspicious;
        Suspiciousness totalSuspicion = GameManager.I.GetTotalSuspicion();
        SensitivityLevel sensitivityLevel = GameManager.I.GetCurrentSensitivity();

        recentlySawSuspicious = Toolbox.Max(recentlySawSuspicious, totalSuspicion);
        Reaction reaction = GameManager.I.GetSuspicionReaction(recentlySawSuspicious);

        Reaction unmodifiedReaction = GameManager.I.GetSuspicionReaction(recentlySawSuspicious, applyModifiers: false);
        if (unmodifiedReaction == Reaction.attack && GameManager.I.gameData.levelState.anyAlarmActive()) {
            GameManager.I.ActivateHQRadio();
        }

        if (reaction == Reaction.investigate && recentlyInCombat) {
            reaction = Reaction.attack;
        }
        return reaction;
    }

    public void OnPoolActivate() {
        Awake();
        listener = gameObject.GetComponentInChildren<Listener>();
    }
    public void OnPoolDectivate() {
        perceptionCountdown = 0f;
        lastSeenPlayerPosition = null;
        lastHeardDisturbancePosition = null;
        lastHeardPlayerPosition = null;

        lastGunshotHeard = null;
        timeSinceLastSeen = 0f;
        playerCollider = null;
        // alertness = Alertness.normal;
        recentlyInCombat = false;
        recentHeardSuspicious = Suspiciousness.normal;
        recentlySawSuspicious = Suspiciousness.normal;
        stateMachine = new SphereRobotBrain();
        EnterDefaultState();
        characterHurtable.OnHitStateChanged -= ((IHitstateSubscriber)this).HandleHurtableChanged;
        // TODO: reset gun state...?!?
    }

    public DialogueInput GetDialogueInput() => GameManager.I.GetDialogueInput(this);

#if UNITY_EDITOR
    void OnDrawGizmos() {
        if (stateMachine != null) {
            string labelText = $"state: {stateMachine.currentStateName}";
            Handles.Label(transform.position, labelText);

            string customName = "Relic\\MaskedSpider.png";
            Gizmos.DrawIcon(getLocationOfInterest(), customName, true);
        }
    }
#endif
}
