using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Easings;
using UnityEngine;
using UnityEngine.AI;
public enum Reaction { ignore, attack, investigate }

public class SphereRobotAI : IBinder<SightCone>, IDamageReceiver, IListener, IHitstateSubscriber, IPoolable {
    public Destructible characterHurtable;
    public HitState hitState { get; set; }
    public SightCone sightCone;
    public Transform sightOrigin;
    public NavMeshPath navMeshPath; // TODO: remove this
    public GameObject controllable;
    public IInputReceiver sphereController;
    public GunHandler gunHandler;
    public AlertHandler alertHandler;
    public SphereRobotBrain stateMachine;
    float perceptionCountdown;
    public SphereCollider patrolZone;
    readonly float PERCEPTION_INTERVAL = 0.05f;
    readonly float MAXIMUM_SIGHT_RANGE = 50f;
    readonly float LOCK_ON_TIME = 0.5f;
    public Vector3 lastSeenPlayerPosition;
    public float timeSinceLastSeen;
    public Collider playerCollider;
    public Alertness alertness = Alertness.normal;
    public bool recentlyInCombat;
    public Suspiciousness recentHeardSuspicious;
    public Suspiciousness recentlySawSuspicious;
    public PatrolRoute patrolRoute;
    public bool skipShootAnimation;
    NoiseComponent lastGunshotHeard;
    Damage lastDamage;

    private void OnDrawGizmos() {
        string customName = "Relic\\MaskedSpider.png";
        Gizmos.DrawIcon(lastSeenPlayerPosition, customName, true);
    }
    void Start() {
        sphereController = controllable.GetComponent<IInputReceiver>();

        // TODO: change this hack!
        // TODO: save / load gun state
        TorsoAnimation torsoAnimation = controllable.GetComponentInChildren<TorsoAnimation>();
        LegsAnimation legsAnimation = controllable.GetComponentInChildren<LegsAnimation>();
        HeadAnimation headAnimation = controllable.GetComponentInChildren<HeadAnimation>();
        if (torsoAnimation != null) {
            PlayerData data = PlayerData.DefaultGameData();
            foreach (ISaveable saveable in controllable.GetComponentsInChildren<ISaveable>()) {
                saveable.LoadState(data);
            }
        }
        gunHandler.primary = new GunInstance(Gun.Load("smg"));
        gunHandler.SwitchToGun(1);

        alertHandler.Hide();

        Bind(sightCone.gameObject);
        stateMachine = new SphereRobotBrain();
        navMeshPath = new NavMeshPath();
        EnterDefaultState();
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

    public void StateFinished(SphereControlState routine) {
        switch (routine) {
            default:
            case ReactToAttackState:
                // TODO: do something different. we were just attacked
                if (lastDamage != null) {
                    ChangeState(new SearchDirectionState(this, lastDamage, doIntro: false));
                } else if (lastGunshotHeard != null) {
                    ChangeState(new SearchDirectionState(this, lastGunshotHeard, doIntro: false));
                } else {
                    EnterDefaultState();
                }
                break;
            case SearchDirectionState:

                alertHandler.ShowGiveUp();
                // speechTextController.HaltSpeechForTime(2f);
                // if (recentlyInCombat) {
                //     
                // } else 
                // if (lastDamage != null) {
                //     // TODO: we were just in combat. report to HQ?
                //     ChangeState(new SearchDirectionState(this, lastDamage, doIntro: false));
                // } else if (lastGunshotHeard != null) {
                //     ChangeState(new SearchDirectionState(this, lastGunshotHeard, doIntro: false));
                // } else {
                EnterDefaultState();
                // }
                break;
            case SphereAttackState:
                // TODO: we were just in combat. report to HQ
                if (lastDamage != null) {
                    // TODO: we were just in combat. report to HQ?
                    ChangeState(new SearchDirectionState(this, lastDamage, doIntro: false));
                } else if (lastGunshotHeard != null) {
                    ChangeState(new SearchDirectionState(this, lastGunshotHeard, doIntro: false));
                } else {
                    EnterDefaultState();
                }
                break;
        }
    }
    private void ChangeState(SphereControlState routine) {
        stateMachine.ChangeState(routine);
        Debug.Log($"changing state {routine}");
        switch (routine) {
            case SearchDirectionState search:
                break;
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
            stateMachine.currentState.OnObjectPerceived(other);
        }
    }

    void PerceivePlayerObject(Collider other) {
        float distance = Vector3.Distance(transform.position, other.bounds.center);
        if (GameManager.I.IsPlayerVisible(distance)) {
            stateMachine.currentState.OnObjectPerceived(other);
            lastSeenPlayerPosition = other.bounds.center;
            timeSinceLastSeen = 0f;
            playerCollider = other;
            Reaction reaction = ReactToPlayerSuspicion();
            if (reaction == Reaction.attack || reaction == Reaction.investigate) { // TODO: investigate routine
                switch (stateMachine.currentState) {
                    case SearchDirectionState:
                    case SphereMoveState:
                    case SpherePatrolState:
                    case ReactToAttackState:
                        alertHandler.ShowAlert();
                        ChangeState(new SphereAttackState(this, gunHandler));
                        break;
                }
            } else if (reaction == Reaction.investigate) {
                alertHandler.ShowWarn();
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
                // return other == hit.collider;
            }
        }
        // Vector3 direction = other.bounds.center - position;

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
                alertHandler.ShowAlert(useWarnMaterial: true);
                ChangeState(new ReactToAttackState(this, damage));
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
        if (noise.data.isGunshot) {
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
                case SearchDirectionState:
                    alertHandler.ShowAlert(useWarnMaterial: true);
                    ChangeState(new ReactToAttackState(this, noise));
                    break;
            }
        } else if (noise.data.player) {
            if (noise.data.suspiciousness > Suspiciousness.normal) {
                SuspicionRecord record = new SuspicionRecord {
                    content = "a suspicious noise was heard",
                    // suspiciousness = noise.data.suspiciousness,
                    suspiciousness = Suspiciousness.suspicious,
                    lifetime = 10f,
                    maxLifetime = 10f
                };
                GameManager.I.AddSuspicionRecord(record);

                switch (stateMachine.currentState) {
                    case SphereMoveState:
                    case SpherePatrolState:
                        alertHandler.ShowWarn();
                        ChangeState(new SearchDirectionState(this, noise));
                        break;
                    case SearchDirectionState:
                        if (stateMachine.timeInCurrentState > 3f)
                            ChangeState(new SearchDirectionState(this, noise, doIntro: false));
                        break;
                }
            }
        } else {
            // not player
            if (noise.data.suspiciousness > Suspiciousness.normal) {
                switch (stateMachine.currentState) {
                    case SphereMoveState:
                    case SpherePatrolState:
                        alertHandler.ShowWarn();
                        ChangeState(new SearchDirectionState(this, noise));
                        break;
                }
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
        if (reaction == Reaction.investigate && recentlyInCombat) {
            reaction = Reaction.attack;
        }
        return reaction;
    }

    public void OnPoolActivate() {

    }
    public void OnPoolDectivate() {
        perceptionCountdown = 0f;
        lastSeenPlayerPosition = Vector3.zero;
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
