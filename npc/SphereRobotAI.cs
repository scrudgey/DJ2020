using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Easings;
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
    public GameObject controllable;
    public IInputReceiver sphereController;
    public GunHandler gunHandler;
    public AlertHandler alertHandler;
    public SphereRobotBrain stateMachine;
    public SpeechTextController speechTextController;
    float perceptionCountdown;
    public SphereCollider patrolZone;
    readonly float PERCEPTION_INTERVAL = 0.05f;
    readonly float MAXIMUM_SIGHT_RANGE = 50f;
    readonly float LOCK_ON_TIME = 0.5f;
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
    public bool overrideDefaultState;
    private float footstepImpulse;

    void Start() {
        sphereController = controllable.GetComponent<IInputReceiver>();
        alertHandler.Hide();
        Bind(sightCone.gameObject);
        navMeshPath = new NavMeshPath();
        if (!overrideDefaultState) {
            stateMachine = new SphereRobotBrain();
            EnterDefaultState();
        }
        if (characterHurtable != null) {
            characterHurtable.OnHitStateChanged += ((IHitstateSubscriber)this).HandleHurtableChanged;
        }
    }
    override public void OnDestroy() {
        base.OnDestroy();
        characterHurtable.OnHitStateChanged -= ((IHitstateSubscriber)this).HandleHurtableChanged;
    }

    void EnterDefaultState() {
        if (patrolRoute != null) {
            ChangeState(new SpherePatrolState(this, patrolRoute));
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
            case ReactToAttackState:
                // TODO: do something different. we were just attacked
                if (lastDamage != null) {
                    ChangeState(new SearchDirectionState(this, lastDamage, doIntro: false));
                } else if (getLocationOfInterest() != Vector3.zero) {
                    ChangeState(new SearchDirectionState(this, getLocationOfInterest(), doIntro: false));
                } else {
                    EnterDefaultState();
                }
                break;
            case StopAndListenState:
                StopAndListenState listenState = (StopAndListenState)routine;
                ChangeState(listenState.getNextState());
                // listener.SetListenRadius();
                break;
            case SearchDirectionState:
                alertHandler.ShowGiveUp();
                EnterDefaultState();
                // listener.SetListenRadius();
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
                    ChangeState(new SearchDirectionState(this, lastDamage, doIntro: false));
                } else if (getLocationOfInterest() != Vector3.zero) {
                    ChangeState(new SearchDirectionState(this, getLocationOfInterest(), doIntro: false));
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
            case StopAndListenState:
            case SearchDirectionState:
                // listener.SetListenRadius(radius: 2f);
                break;
            case ReportToHQState:
            case ReactToAttackState:
            case SphereAttackState attack:
                recentlyInCombat = true;
                break;
        }
    }
    void Update() {
        if (hitState == HitState.dead) {
            return;
        }
        timeSinceLastSeen += Time.deltaTime;
        PlayerInput input = stateMachine.Update();
        input.preventWallPress = true;

        // possibly avoid bunching here
        float avoidFactor = 0.1f;
        float avoidRadius = 2f;
        Collider[] others = Physics.OverlapSphere(transform.position, avoidRadius, LayerUtil.GetMask(Layer.obj));
        Vector3 closeness = Vector3.zero;
        foreach (Collider collider in others) {
            if (collider == null || collider.gameObject == null)
                continue;
            if (collider.transform.IsChildOf(transform))
                continue;
            SphereRobotAI otherAI = collider.GetComponent<SphereRobotAI>();
            if (otherAI != null) {
                closeness += transform.position - otherAI.transform.position;
            }
        }
        closeness.y = 0;
        input.moveDirection += avoidFactor * closeness;

        SetInputs(input);
        perceptionCountdown -= Time.deltaTime;
        if (perceptionCountdown <= 0) {
            perceptionCountdown += PERCEPTION_INTERVAL;
            PerceiveFieldOfView();
        }
        if (timeSinceLastSeen < LOCK_ON_TIME && playerCollider != null) {
            if (Vector3.Dot(target.transform.up, playerCollider.transform.position - transform.position) < 0) {
                if (TargetVisible(playerCollider))
                    Perceive(playerCollider);
            }
        }
        if (footstepImpulse > 0f) {
            footstepImpulse -= Time.deltaTime;
        }
        for (int i = 0; i < navMeshPath.corners.Length - 1; i++) {
            Debug.DrawLine(navMeshPath.corners[i], navMeshPath.corners[i + 1], Color.white);
        }
    }
    void SetInputs(PlayerInput input) {
        sphereController.SetInputs(input);
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
            if (stateMachine.currentState != null)
                stateMachine.currentState.OnObjectPerceived(other);
        }
    }

    void PerceivePlayerObject(Collider other) {
        float distance = Vector3.Distance(transform.position, other.bounds.center);
        if (GameManager.I.IsPlayerVisible(distance)) {
            stateMachine.currentState.OnObjectPerceived(other);
            lastSeenPlayerPosition = new SpaceTimePoint(other.bounds.center);
            timeSinceLastSeen = 0f;
            playerCollider = other;
            Reaction reaction = ReactToPlayerSuspicion();
            if (reaction == Reaction.attack || reaction == Reaction.investigate) { // TODO: investigate routine
                switch (stateMachine.currentState) {
                    case SearchDirectionState:
                    case SphereMoveState:
                    case SpherePatrolState:
                    case ReactToAttackState:
                    case FollowTheLeaderState:
                    case StopAndListenState:
                        alertHandler.ShowAlert();
                        ChangeState(new SphereAttackState(this, gunHandler));
                        break;
                }
            } else if (reaction == Reaction.investigate) {
                alertHandler.ShowWarn();
            } else if (reaction == Reaction.ignore) {
                switch (stateMachine.currentState) {
                    case SearchDirectionState:
                        // TODO: pause for a minute here
                        alertHandler.ShowGiveUp();
                        EnterDefaultState();
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

    bool TargetVisible(Collider other) {
        Vector3 position = sightOrigin.position; // TODO: configurable
        Vector3[] directions = new Vector3[]{
            other.bounds.center - position,
            (other.bounds.center + other.bounds.extents) - position,
            (other.bounds.center - other.bounds.extents) - position,
        };
        foreach (Vector3 direction in directions) {
            Ray ray = new Ray(position, direction);
            RaycastHit[] hits = Physics.RaycastAll(ray, MAXIMUM_SIGHT_RANGE, LayerUtil.GetMask(Layer.def, Layer.obj));
            foreach (RaycastHit hit in hits.OrderBy(h => h.distance)) {
                if (hit.transform.IsChildOf(transform.root))
                    continue;
                TagSystemData tagData = Toolbox.GetTagData(hit.collider.gameObject);
                if (tagData.bulletPassthrough) continue;

                Color color = other == hit.collider ? Color.yellow : Color.red;
                Debug.DrawLine(position, hit.collider.bounds.center, color, 0.5f);
                if (other == hit.collider) {
                    return true;
                } else break;
            }
        }
        return false;
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
                alertHandler.ShowAlert(useWarnMaterial: true);
                if (GameManager.I.gameData.levelState.anyAlarmActive()) {
                    ChangeState(new SearchDirectionState(this, damage, doIntro: false));
                } else {
                    ChangeState(new ReactToAttackState(this, speechTextController, damage));
                }
                break;
        }
        return DamageResult.NONE;
    }

    public void HearNoise(NoiseComponent noise) {
        if (noise == null)
            return;
        recentHeardSuspicious = Toolbox.Max<Suspiciousness>(recentHeardSuspicious, noise.data.suspiciousness);

        if (stateMachine != null && stateMachine.currentState != null)
            stateMachine.currentState.OnNoiseHeard(noise);

        if (noise.data.isGunshot && noise.data.suspiciousness > Suspiciousness.suspicious) {
            if (noise.data.player) {
                lastHeardPlayerPosition = new SpaceTimePoint(noise.transform.position);
            } else {
                lastHeardDisturbancePosition = new SpaceTimePoint(noise.transform.position);
            }
            lastGunshotHeard = noise;
            SuspicionRecord record = new SuspicionRecord {
                content = "gunshots heard",
                suspiciousness = Suspiciousness.aggressive,
                lifetime = 60f,
                maxLifetime = 60f
            };
            GameManager.I.AddSuspicionRecord(record);
            switch (stateMachine.currentState) {
                case SphereMoveState:
                case SpherePatrolState:
                case FollowTheLeaderState:
                case DisableAlarmState:
                    alertHandler.ShowAlert(useWarnMaterial: true);
                    if (GameManager.I.gameData.levelState.anyAlarmActive()) {
                        ChangeState(new SearchDirectionState(this, noise, doIntro: false));
                    } else {
                        ChangeState(new ReactToAttackState(this, speechTextController, noise));
                    }
                    break;
                case SearchDirectionState:
                    ChangeState(new SearchDirectionState(this, noise, doIntro: false));
                    break;
            }
        } else if (noise.data.player) {
            lastHeardPlayerPosition = new SpaceTimePoint(noise.transform.position);
            if (noise.data.suspiciousness > Suspiciousness.normal) {
                SuspicionRecord record = new SuspicionRecord {
                    content = "a suspicious noise was heard",
                    suspiciousness = Suspiciousness.suspicious,
                    lifetime = 10f,
                    maxLifetime = 10f
                };
                GameManager.I.AddSuspicionRecord(record);
                switch (stateMachine.currentState) {
                    case SphereMoveState:
                    case SpherePatrolState:
                    case FollowTheLeaderState:
                    case DisableAlarmState:
                        alertHandler.ShowWarn();
                        ChangeState(new SearchDirectionState(this, noise));
                        break;
                    case SearchDirectionState:
                        // if (stateMachine.timeInCurrentState > 3f)
                        ChangeState(new SearchDirectionState(this, noise, doIntro: false));
                        break;
                }
            } else if (noise.data.isFootsteps) {
                HandleFootstepNoise(noise);
            }
        } else {
            // not player
            if (noise.data.suspiciousness > Suspiciousness.normal) {
                if (noise.data.suspiciousness == Suspiciousness.suspicious) {
                    SuspicionRecord record = new SuspicionRecord {
                        content = "a suspicious noise was heard",
                        suspiciousness = Suspiciousness.suspicious,
                        lifetime = 10f,
                        maxLifetime = 10f
                    };
                    GameManager.I.AddSuspicionRecord(record);

                    switch (stateMachine.currentState) {
                        case SphereMoveState:
                        case SpherePatrolState:
                        case FollowTheLeaderState:
                        case DisableAlarmState:
                            alertHandler.ShowWarn();
                            ChangeState(new SearchDirectionState(this, noise));
                            break;
                    }
                } else if (noise.data.suspiciousness == Suspiciousness.aggressive) {
                    SuspicionRecord record = new SuspicionRecord {
                        content = "an explosion was heard",
                        suspiciousness = Suspiciousness.aggressive,
                        lifetime = 60f,
                        maxLifetime = 60f
                    };
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
        bool thresholded = footstepImpulse > 4f;
        if (GameManager.I.gameData.levelState.template.sensitivityLevel == SensitivityLevel.publicProperty) {
            if (thresholded && recentlyInCombat) {
                switch (stateMachine.currentState) {
                    case SphereMoveState:
                    case SpherePatrolState:
                    case FollowTheLeaderState:
                        alertHandler.ShowWarn();
                        ChangeState(new StopAndListenState(this, stateMachine.currentState, speechTextController));
                        break;
                }
            }
        } else if (GameManager.I.gameData.levelState.template.sensitivityLevel >= SensitivityLevel.privateProperty) {
            if (thresholded)
                switch (stateMachine.currentState) {
                    case SphereMoveState:
                    case SpherePatrolState:
                    case FollowTheLeaderState:
                        alertHandler.ShowWarn();
                        ChangeState(new StopAndListenState(this, stateMachine.currentState, speechTextController));
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

        // TODO: reset gun state...?!?
    }
}
