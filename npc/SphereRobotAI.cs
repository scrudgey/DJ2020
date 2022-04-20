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
    public SphereRobotController sphereController;
    public GunHandler gunHandler;
    public AlertHandler alertHandler;
    private SphereRobotBrain stateMachine;
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
    public Suspiciousness recentHeardSuspicious;  // TODO: set this value
    public Suspiciousness recentlySawSuspicious; // TODO: set this value

    // public TextMeshProU
    private void OnDrawGizmos() {
        string customName = "Relic\\MaskedSpider.png";
        Gizmos.DrawIcon(lastSeenPlayerPosition, customName, true);
    }
    void Start() {
        // alertIcon.enabled = false;
        alertHandler.Hide();
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
        switch (routine) {
            case SphereAttackRoutine attack:
                recentlyInCombat = true;
                break;
        }
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

            Reaction reaction = ReactToPlayerSuspicion();

            if (reaction == Reaction.attack || reaction == Reaction.investigate) {
                switch (stateMachine.currentState) {
                    case SearchDirectionState:
                    case SphereMoveRoutine:
                        alertHandler.ShowAlert();
                        ChangeState(new SphereAttackRoutine(this, gunHandler));
                        break;
                }
            } else if (reaction == Reaction.investigate) {
                alertHandler.ShowWarn();
                Debug.Log("investigate");
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
        recentHeardSuspicious = Toolbox.Max<Suspiciousness>(recentHeardSuspicious, noise.data.suspiciousness);
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

    public Reaction ReactToPlayerSuspicion() {
        recentlySawSuspicious = Toolbox.Max<Suspiciousness>(recentlySawSuspicious, GameManager.I.PlayerAppearance());
        recentlySawSuspicious = Toolbox.Max<Suspiciousness>(recentlySawSuspicious, GameManager.I.playerInteractor?.GetSuspiciousness() ?? Suspiciousness.normal);
        recentlySawSuspicious = Toolbox.Max<Suspiciousness>(recentlySawSuspicious, GameManager.I.playerItemHandler?.GetSuspiciousness() ?? Suspiciousness.normal);

        // player character: appearance and activity
        Suspiciousness playerActivity = Toolbox.Max<Suspiciousness>(GameManager.I.playerInteractor?.GetSuspiciousness() ?? Suspiciousness.normal, GameManager.I.playerItemHandler?.GetSuspiciousness() ?? Suspiciousness.normal);
        if (GameManager.I.gameData.levelData.sensitivityLevel == SensitivityLevel.publicProperty) {

            // guard AI: focus
            // alertness;
            // AI: state of knowledge
            // recentHeardSuspicious;
            // recentlySawSuspicious;

            if (GameManager.I.PlayerAppearance() == Suspiciousness.aggressive) {
                return Reaction.attack;
            } else if (playerActivity == Suspiciousness.aggressive) {
                return Reaction.attack;
            } else if (GameManager.I.PlayerAppearance() == Suspiciousness.suspicious) {
                if (recentlyInCombat)
                    return Reaction.attack;
                return Reaction.investigate;
            } else if (playerActivity == Suspiciousness.suspicious) {
                if (recentlyInCombat)
                    return Reaction.attack;
                return Reaction.investigate;
            }
            return Reaction.ignore;
        } else if (GameManager.I.gameData.levelData.sensitivityLevel == SensitivityLevel.semiprivateProperty) {
            if (GameManager.I.PlayerAppearance() == Suspiciousness.aggressive) {
                return Reaction.attack;
            } else if (playerActivity == Suspiciousness.aggressive) {
                return Reaction.attack;
            } else if (GameManager.I.PlayerAppearance() == Suspiciousness.suspicious) {
                if (recentlyInCombat || alertness == Alertness.alert || recentHeardSuspicious >= Suspiciousness.suspicious || recentlySawSuspicious >= Suspiciousness.suspicious)
                    return Reaction.attack;
                return Reaction.investigate;
            } else if (playerActivity == Suspiciousness.suspicious) {
                if (recentlyInCombat || alertness == Alertness.alert || recentHeardSuspicious >= Suspiciousness.suspicious || recentlySawSuspicious >= Suspiciousness.suspicious)
                    return Reaction.attack;
                return Reaction.investigate;
            }
            return Reaction.ignore;
        } else if (GameManager.I.gameData.levelData.sensitivityLevel == SensitivityLevel.privateProperty) {
            if (GameManager.I.PlayerAppearance() == Suspiciousness.aggressive) {
                return Reaction.attack;
            } else if (playerActivity == Suspiciousness.aggressive) {
                return Reaction.attack;
            } else if (GameManager.I.PlayerAppearance() == Suspiciousness.suspicious) {
                return Reaction.attack;
            } else if (playerActivity == Suspiciousness.suspicious) {
                return Reaction.attack;
            }
            return Reaction.investigate;
        } else if (GameManager.I.gameData.levelData.sensitivityLevel == SensitivityLevel.restrictedProperty) {
            return Reaction.attack;
        }
        return Reaction.ignore;
    }
}
