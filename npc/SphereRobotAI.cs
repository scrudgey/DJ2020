using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Easings;
using UnityEngine;
using UnityEngine.AI;
public enum Reaction { ignore, attack, investigate }

public class SphereRobotAI : IBinder<SightCone>, IDamageable, IListener {
    public SightCone sightCone;
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
    public Vector3 lastSeenPlayerPosition;
    public float timeSinceLastSeen;
    public Collider playerCollider;
    public Alertness alertness = Alertness.normal;
    public bool recentlyInCombat;
    public Suspiciousness recentHeardSuspicious;
    public Suspiciousness recentlySawSuspicious;
    public PatrolRoute patrolRoute;
    public bool skipShootAnimation;

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

        alertHandler.Hide();
        gunHandler.primary = new GunInstance(Gun.Load("smg"));
        gunHandler.SwitchToGun(1);
        if (skipShootAnimation) {
            gunHandler.Reload();
            gunHandler.ClipIn();
            gunHandler.Rack();
        } else {
            gunHandler.DoReload();
        }

        Bind(sightCone.gameObject);
        stateMachine = new SphereRobotBrain();
        navMeshPath = new NavMeshPath();

        EnterDefaultState();
    }

    void EnterDefaultState() {
        if (patrolRoute != null) {
            ChangeState(new SpherePatrolState(this, patrolRoute));
        } else {
            ChangeState(new SphereMoveState(this, patrolZone));
        }
    }


    public void StateFinished(SphereControlState routine) {
        switch (routine) {
            default:
            case SearchDirectionState:
                alertHandler.ShowGiveUp();
                speechTextController.HaltSpeechForTime(2f);
                // ChangeState(new SphereMoveState(this, patrolZone));
                EnterDefaultState();
                break;
            case SphereAttackState:
                EnterDefaultState();
                // ChangeState(new SphereMoveState(this, patrolZone));
                break;
        }
    }
    private void ChangeState(SphereControlState routine) {
        stateMachine.ChangeState(routine);
        switch (routine) {
            case SearchDirectionState search:
                Debug.Log("enter search direction state");
                break;
            case SphereAttackState attack:
                recentlyInCombat = true;
                break;
        }
    }
    void Update() {
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
        gunHandler.ProcessGunSwitch(input);
        gunHandler.SetInputs(input, skipAnimation: skipShootAnimation);
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
            case SphereMoveState:
            case SpherePatrolState:
                ChangeState(new SearchDirectionState(this, damage));
                break;
            case SearchDirectionState:
                if (stateMachine.timeInCurrentState > 1f)
                    ChangeState(new SearchDirectionState(this, damage));
                break;
        }
    }

    public void HearNoise(NoiseComponent noise) {
        if (noise == null)
            return;
        recentHeardSuspicious = Toolbox.Max<Suspiciousness>(recentHeardSuspicious, noise.data.suspiciousness);
        // Debug.Log(recentHeardSuspicious);
        if (stateMachine != null && stateMachine.currentState != null)
            stateMachine.currentState.OnNoiseHeard(noise);
        if (noise.data.suspiciousness > Suspiciousness.normal && noise.data.player) {
            switch (stateMachine.currentState) {
                case SphereMoveState:
                case SpherePatrolState:
                    ChangeState(new SearchDirectionState(this, noise));
                    break;
                case SearchDirectionState:
                    if (stateMachine.timeInCurrentState > 3f)
                        ChangeState(new SearchDirectionState(this, noise, doIntro: false));
                    break;
            }
        }
    }

    public Reaction ReactToPlayerSuspicion() {
        SuspicionData data = GameManager.I.GetSuspicionData();

        recentlySawSuspicious = new List<Suspiciousness>{
            data.appearanceSuspicion,
            GameManager.I.playerInteractor?.GetSuspiciousness() ?? Suspiciousness.normal,
            GameManager.I.playerItemHandler?.GetSuspiciousness() ?? Suspiciousness.normal
        }.Aggregate(recentlySawSuspicious, Toolbox.Max<Suspiciousness>);

        if (data.levelSensitivity == SensitivityLevel.publicProperty) {
            // guard AI: focus
            // alertness;
            // AI: state of knowledge
            // recentHeardSuspicious;
            // recentlySawSuspicious;

            if (data.appearanceSuspicion == Suspiciousness.aggressive) {
                return Reaction.attack;
            } else if (data.playerActivity() == Suspiciousness.aggressive) {
                return Reaction.attack;
            } else if (data.appearanceSuspicion == Suspiciousness.suspicious) {
                if (recentlyInCombat)
                    return Reaction.attack;
                return Reaction.investigate;
            } else if (data.playerActivity() == Suspiciousness.suspicious) {
                if (recentlyInCombat)
                    return Reaction.attack;
                return Reaction.investigate;
            }
            return Reaction.ignore;
        } else if (data.levelSensitivity == SensitivityLevel.semiprivateProperty) {
            if (data.appearanceSuspicion == Suspiciousness.aggressive) {
                return Reaction.attack;
            } else if (data.playerActivity() == Suspiciousness.aggressive) {
                return Reaction.attack;
            } else if (data.appearanceSuspicion == Suspiciousness.suspicious) {
                if (recentlyInCombat || alertness == Alertness.alert || recentHeardSuspicious >= Suspiciousness.suspicious || recentlySawSuspicious >= Suspiciousness.suspicious)
                    return Reaction.attack;
                return Reaction.investigate;
            } else if (data.playerActivity() == Suspiciousness.suspicious) {
                if (recentlyInCombat || alertness == Alertness.alert || recentHeardSuspicious >= Suspiciousness.suspicious || recentlySawSuspicious >= Suspiciousness.suspicious)
                    return Reaction.attack;
                return Reaction.investigate;
            }
            return Reaction.ignore;
        } else if (data.levelSensitivity == SensitivityLevel.privateProperty) {
            if (data.appearanceSuspicion == Suspiciousness.aggressive) {
                return Reaction.attack;
            } else if (data.playerActivity() == Suspiciousness.aggressive) {
                return Reaction.attack;
            } else if (data.appearanceSuspicion == Suspiciousness.suspicious) {
                return Reaction.attack;
            } else if (data.playerActivity() == Suspiciousness.suspicious) {
                return Reaction.attack;
            }
            return Reaction.investigate;
        } else if (data.levelSensitivity == SensitivityLevel.restrictedProperty) {
            return Reaction.attack;
        }
        return Reaction.ignore;
    }
}
