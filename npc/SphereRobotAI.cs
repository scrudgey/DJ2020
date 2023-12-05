using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AI;
using Easings;
using KinematicCharacterController;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
public enum Reaction { ignore, attack, investigate }

public class SphereRobotAI : IBinder<SightCone>, IDamageReceiver, IListener, IHitstateSubscriber, IPoolable {
    public string dialogueName;
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
    public SpottedHighlight highlight;

    float perceptionCountdown;
    public SphereCollider patrolZone;
    readonly float PERCEPTION_INTERVAL = 0.025f;
    readonly float MAXIMUM_SIGHT_RANGE = 25f;
    readonly float LOCK_ON_TIME = 0.5f;
    float timeSinceInvestigatedFootsteps;
    float timeSinceInterrogatedStranger;
    public float timeSinceLastSeen;
    public Collider seenPlayerCollider;
    public Alertness alertness = Alertness.normal;
    public bool recentlyInCombat;
    public Suspiciousness recentHeardSuspicious;
    public Suspiciousness lastSuspicionLevel;
    public PatrolRoute patrolRoute;
    public bool skipShootAnimation;
    NoiseComponent lastGunshotHeard;
    public SpaceTimePoint lastSeenPlayerPosition;
    public SpaceTimePoint lastHeardPlayerPosition;
    public SpaceTimePoint lastHeardDisturbancePosition;
    Damage lastDamage;
    // public bool overrideDefaultState;
    private float footstepImpulse;
    public SpeechEtiquette[] etiquettes;
    // public Sprite portrait;
    bool awareOfCorpse;
    Collider[] nearbyOthers;
    RaycastHit[] raycastHits;
    public HashSet<int> physicalKeys;
    Vector3 closeness;
    List<Transform> otherTransforms = new List<Transform>();
    ClearPoint[] clearPoints;
    static readonly float avoidFactor = 5f;
    static readonly float avoidRadius = 0.2f;
    public bool isStrikeTeamMember;
    public PrefabPool prefabPool;
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
            characterHurtable.OnHitStateChanged += HandleHitStateChange;
        }
        if (speechTextController == null) {
            GameObject obj = GameObject.Instantiate(Resources.Load("prefabs/speechOverlay")) as GameObject;
            SpeechTextController controller = obj.GetComponent<SpeechTextController>();
            this.speechTextController = controller;
            controller.followTransform = transform;
        }
        if (highlight == null) {
            GameObject obj = GameObject.Instantiate(Resources.Load("prefabs/spottedHighlight")) as GameObject;
            SpottedHighlight spottedHighlight = obj.GetComponent<SpottedHighlight>();
            this.highlight = spottedHighlight;
            highlight.followTransform = transform;
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
        characterHurtable.OnHitStateChanged -= HandleHitStateChange;
    }

    void HandleHitStateChange(Destructible destructible) {
        if (destructible.hitState == HitState.dead) {
            alertHandler.enabled = false;
            speechTextController.enabled = false;
        }
    }

    void EnterDefaultState() {
        if (isStrikeTeamMember) {
            switch (GameManager.I.gameData.levelState.delta.strikeTeamBehavior) {
                case LevelTemplate.StrikeTeamResponseBehavior.clear:
                    ClearPoint[] allClearPoints = FindObjectsOfType<ClearPoint>();
                    // foreach (ClearPoint point in allClearPoints) {
                    //     point.cleared = false;
                    // }
                    if (allClearPoints.Length > 0) {
                        ChangeState(new SphereClearPointsState(this, characterController, allClearPoints));
                    } else {
                        ChangeState(new SearchDirectionState(this, GameManager.I.locationOfLastDisturbance, characterController, doIntro: false));
                    }
                    break;
                default:
                case LevelTemplate.StrikeTeamResponseBehavior.investigate:
                    // ChangeState(new SearchDirectionState(this, GameManager.I.locationOfLastDisturbance, characterController, doIntro: false));
                    ChangeState(new SpherePatrolState(this, patrolRoute, characterController));
                    break;
            }
        } else if (patrolRoute != null) {
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

    public void StateFinished(SphereControlState routine, TaskState result) {
        switch (routine) {
            default:
                EnterDefaultState();
                break;
            case SphereExitLevelState:
                GameManager.I.RemoveNPC(characterController);
                prefabPool.RecallObject(transform.root.gameObject);
                break;
            case SphereClearPointsState:
                if (isStrikeTeamMember &&
                    GameManager.I.gameData.levelState.template.strikeCompletionThreshold == LevelTemplate.StrikeTeamCompletionThreshold.clear) {
                    GameManager.I.StrikeTeamMissionComplete();
                } else {
                    EnterDefaultState();
                }
                break;
            case ReportGunshotsState:
                ChangeState(new SearchDirectionState(this, lastGunshotHeard, characterController, doIntro: false, speedCoefficient: 2f));
                break;
            case SphereHoldAtGunpointState:
                SphereHoldAtGunpointState holdAtGunpointState = (SphereHoldAtGunpointState)routine;
                if (holdAtGunpointState.isPlayerSuspicious()) {
                    ChangeState(new SphereAttackState(this, gunHandler, characterController, speechTextController));
                } else ChangeState(new SphereInvestigateState(this, highlight, characterController, speechTextController));
                break;
            case SphereInvestigateState:
                timeSinceInterrogatedStranger = 120f;
                highlight.target = null;
                SphereInvestigateState investigateState = (SphereInvestigateState)routine;
                if (investigateState.dialogueResult == NeoDialogueMenu.DialogueResult.fail) {
                    // TODO: why fails?
                    ChangeState(new SphereHoldAtGunpointState(this, speechTextController));
                } else if (investigateState.dialogueResult == NeoDialogueMenu.DialogueResult.stun) {
                    alertHandler.ShowWarn();
                    ChangeState(new StunState(this));
                } else if (investigateState.isPlayerAggressive()) {
                    alertHandler.ShowWarn();
                    ChangeState(new SphereHoldAtGunpointState(this, speechTextController));
                } else if (investigateState.isPlayerSuspicious()) {
                    alertHandler.ShowWarn();
                    ChangeState(new SphereHoldAtGunpointState(this, speechTextController));
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
                if (isStrikeTeamMember &&
                 GameManager.I.gameData.levelState.delta.strikeTeamBehavior == LevelTemplate.StrikeTeamResponseBehavior.investigate &&
                    GameManager.I.gameData.levelState.template.strikeCompletionThreshold == LevelTemplate.StrikeTeamCompletionThreshold.clear) {
                    GameManager.I.StrikeTeamMissionComplete();
                } else {
                    EnterDefaultState();
                }
                break;
            case SphereAttackState:
                if (!GameManager.I.gameData.levelState.anyAlarmTerminalActivated()) {
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
            // SphereRobotAI otherAI = collider.GetComponent<SphereRobotAI>();
            if (collider.CompareTag("actor")) {
                otherTransforms.Add(collider.transform);
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

        // we set playercollider when we perceive player
        if (timeSinceLastSeen < LOCK_ON_TIME && seenPlayerCollider != null) {
            // Debug.Log($"{Vector3.Dot(target.transform.up, playerCollider.bounds.center - transform.position)} {Vector3.Dot(target.transform.up, playerCollider.bounds.center - transform.position) < 0}");
            if (Vector3.Dot(target.transform.up, seenPlayerCollider.bounds.center - transform.position) < 0) {
                SightCheckPlayer(GameManager.I.playerCollider);
            }
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

    void SightCheckPlayer(Collider playerCollider) {
        // Collider player = ;
        ClearLineOfSight(playerCollider, (RaycastHit hit) => {
            // Debug.Log($"sight check player raycast hit: {hit.collider}");
            if (hit.collider == playerCollider) Perceive(playerCollider, byPassVisibilityCheck: true);
        });
    }
    void SetInputs(PlayerInput input) {
        characterController.SetInputs(input);
    }
    public override void HandleValueChanged(SightCone t) {
        if (t.newestAddition != null) {
            ClearLineOfSight(t.newestAddition, (RaycastHit hit) => {
                if (hit.collider == t.newestAddition) Perceive(t.newestAddition);
            });
        }
    }
    void PerceiveFieldOfView() {
        foreach (Collider collider in target.fieldOfView) {
            if (collider == null)
                continue;
            ClearLineOfSight(collider, (RaycastHit hit) => {
                if (hit.collider == collider) Perceive(collider);
            });
        }
    }
    void Perceive(Collider other, bool byPassVisibilityCheck = false) {
        if (other == null) return;
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
                    if (!corpse.reported) {
                        alertHandler.ShowWarn();
                        switch (stateMachine.currentState) {
                            case SearchDirectionState:
                            case SphereMoveState:
                            case SpherePatrolState:
                            case FollowTheLeaderState:
                            case StopAndListenState:
                            case SphereInvestigateState:
                            case SphereClearPointsState:

                                corpse.reported = true;
                                ChangeState(new InvestigateCorpseState(this, corpse, speechTextController, characterController));
                                break;
                        }
                    }
                }
            }
            if (other.CompareTag("bulletImpact")) {
                BulletImpact bulletImpact = other.GetComponent<BulletImpact>();
                if (bulletImpact.damage.hit.collider.transform.IsChildOf(transform)) {

                } else {
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
                        case SphereClearPointsState:
                            alertHandler.ShowAlert(useWarnMaterial: true);
                            SuspicionRecord record = SuspicionRecord.shotSuspicion();
                            GameManager.I.AddSuspicionRecord(record);
                            ChangeState(new ReactToAttackState(this, speechTextController, bulletImpact.damage, characterController, initialPause: 0f));
                            break;
                    }
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
                        case SphereClearPointsState:

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
        if (GameManager.I.playerCharacterController.state == CharacterState.hvac || GameManager.I.playerCharacterController.state == CharacterState.hvacAim)
            return;
        float distance = Vector3.Distance(transform.position, other.bounds.center);
        if (byPassVisibilityCheck || GameManager.I.IsPlayerVisible(distance) || stateMachine.currentState is SphereInvestigateState) {
            stateMachine.currentState.OnObjectPerceived(other);
            lastSeenPlayerPosition = new SpaceTimePoint(other.bounds.center);
            timeSinceLastSeen = 0f;
            seenPlayerCollider = other;
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
                    case SphereClearPointsState:
                        alertHandler.ShowAlert();
                        ChangeState(new SphereAttackState(this, gunHandler, characterController, speechTextController));
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
                    case SphereClearPointsState:

                        if (timeSinceInterrogatedStranger <= 0) {
                            alertHandler.ShowWarn();
                            GameManager.I.StartSpottedCutscene(this);
                            ChangeState(new PauseState(this, new SphereInvestigateState(this, highlight, characterController, speechTextController), 1f));
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

    void ClearLineOfSight(Collider other, Action<RaycastHit> callback) {
        Vector3 position = sightOrigin.position; // TODO: configurable
        Vector3 direction = other.bounds.center - position;
        float distance = direction.magnitude;
        AsyncRaycastService.I.RequestRaycast(position, direction, distance, LayerUtil.GetLayerMask(Layer.def, Layer.obj, Layer.interactive), callback);
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
        // TODO: check hitstate?
        recentHeardSuspicious = Toolbox.Max<Suspiciousness>(recentHeardSuspicious, noise.data.suspiciousness);

        if (stateMachine != null && stateMachine.currentState != null)
            stateMachine.currentState.OnNoiseHeard(noise);

        if (noise.data.isGunshot && noise.data.suspiciousness > Suspiciousness.suspicious) {
            lastHeardDisturbancePosition = new SpaceTimePoint(noise.transform.position);

            if (noise.data.player) {
                lastHeardPlayerPosition = new SpaceTimePoint(noise.transform.position);
                if (Vector3.Dot(target.transform.up, GameManager.I.playerCollider.bounds.center - transform.position) < 0) {
                    SightCheckPlayer(GameManager.I.playerCollider);
                } else {

                }
            }
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
                case SphereClearPointsState:
                    alertHandler.ShowAlert(useWarnMaterial: true);
                    if (GameManager.I.gameData.levelState.anyAlarmTerminalActivated()) {
                        // alarm is already active; no need to handle reporting to HQ. run to firefight.
                        ChangeState(new SearchDirectionState(this, noise, characterController, doIntro: false, speedCoefficient: 2f));
                    } else {
                        if (dotFactor < 1f) {
                            // gunshots not directed at me; report to HQ and then run to battle
                            if (GameManager.I.levelRadioTerminal() != null && !GameManager.I.isAlarmRadioInProgress(gameObject)) {
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
                    case SphereClearPointsState:
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
                        case SphereClearPointsState:
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
                        case SphereClearPointsState:
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

        Suspiciousness playerSuspicion = GameManager.I.GetTotalSuspicion();

        if (GameManager.I.gameData.levelState.template.sensitivityLevel <= SensitivityLevel.semiprivateProperty &&
            reachedFootstepThreshold &&
            recentlyInCombat &&
            notBoredOfFootsteps) {
            switch (stateMachine.currentState) {
                case SphereMoveState:
                case SpherePatrolState:
                case FollowTheLeaderState:
                case SphereClearPointsState:
                    // timeSinceInvestigatedFootsteps = 10f;
                    alertHandler.ShowWarn();
                    ChangeState(new StopAndListenState(this, stateMachine.currentState, speechTextController, characterController));
                    break;
            }
        } else if (GameManager.I.gameData.levelState.template.sensitivityLevel >= SensitivityLevel.privateProperty &&
                playerSuspicion >= Suspiciousness.suspicious &&
                reachedFootstepThreshold &&
                notBoredOfFootsteps) {
            switch (stateMachine.currentState) {
                case SphereMoveState:
                case SpherePatrolState:
                case FollowTheLeaderState:
                case SphereClearPointsState:
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

        lastSuspicionLevel = Toolbox.Max(lastSuspicionLevel, totalSuspicion);
        Reaction reaction = GameManager.I.GetSuspicionReaction(lastSuspicionLevel);

        Reaction unmodifiedReaction = GameManager.I.GetSuspicionReaction(lastSuspicionLevel, applyModifiers: false);
        if (unmodifiedReaction == Reaction.attack && GameManager.I.gameData.levelState.anyAlarmTerminalActivated()) {
            GameManager.I.ActivateHQRadioNode();
        }

        if (reaction == Reaction.investigate && recentlyInCombat) {
            reaction = Reaction.attack;
        }
        return reaction;
    }

    public void OnAlarmActivate(ClearPoint[] allClearPoints) {
        this.clearPoints = allClearPoints;
        if (allClearPoints.Length > 0) {
            switch (stateMachine.currentState) {
                case SpherePatrolState:
                    ChangeState(new SphereClearPointsState(this, characterController, allClearPoints));
                    break;
            }
        }
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
        seenPlayerCollider = null;
        // alertness = Alertness.normal;
        recentlyInCombat = false;
        recentHeardSuspicious = Suspiciousness.normal;
        lastSuspicionLevel = Suspiciousness.normal;
        alertHandler.enabled = true;
        speechTextController.enabled = true;
        highlight.navMeshPath = null;
        stateMachine = new SphereRobotBrain();
        EnterDefaultState();
        characterHurtable.OnHitStateChanged -= ((IHitstateSubscriber)this).HandleHurtableChanged;
        // TODO: reset gun state...?!?
    }
    public DialogueCharacterInput myCharacterInput() => new DialogueCharacterInput {
        portrait = speechTextController.portrait,
        etiquettes = etiquettes,
        alertness = alertness,
        name = dialogueName
    };
    public DialogueInput GetDialogueInput() => GameManager.I.GetDialogueInput(gameObject, myCharacterInput());

    public void TellAboutSuspiciousPlayer(WorkerNPCAI reporter) {
        if (this.lastSeenPlayerPosition != null && reporter.lastSeenPlayerPosition != null && this.lastSeenPlayerPosition.time < reporter.lastSeenPlayerPosition.time)
            this.lastSeenPlayerPosition = reporter.lastSeenPlayerPosition;

        if (this.lastHeardDisturbancePosition != null && reporter.lastHeardDisturbancePosition != null && this.lastHeardDisturbancePosition.time < reporter.lastHeardDisturbancePosition.time)
            this.lastHeardDisturbancePosition = reporter.lastHeardDisturbancePosition;

        if (this.lastHeardPlayerPosition != null && reporter.lastHeardPlayerPosition != null && this.lastHeardPlayerPosition.time < reporter.lastHeardPlayerPosition.time)
            this.lastHeardPlayerPosition = reporter.lastHeardPlayerPosition;

        switch (stateMachine.currentState) {
            case SearchDirectionState:
            case SphereMoveState:
            case SpherePatrolState:
            case FollowTheLeaderState:
            case StopAndListenState:
            case SphereInvestigateState:
            case SphereClearPointsState:
            case DisableAlarmState:
            case ReportToHQState:
            case ReactToTamperState:
            case StunState:
            case PauseState:
                ChangeState(new SearchDirectionState(this, lastSeenPlayerPosition.position, characterController, doIntro: false, speedCoefficient: 2f));
                break;
        }
    }


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
