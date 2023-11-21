using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KinematicCharacterController;
using UnityEngine;
using UnityEngine.AI;

public class CivilianNPCAI : IBinder<SightCone>, IListener, IHitstateSubscriber, IDamageReceiver {
    public Listener listener { get; set; }
    public HitState hitState { get; set; }
    public AlertHandler alertHandler;

    public SightCone sightCone;

    public NavMeshPath navMeshPath;
    public CharacterController characterController;
    public GunHandler gunHandler;
    public CivilianNPCBrain stateMachine;
    public KinematicCharacterMotor motor;
    public SpeechTextController speechTextController;

    Collider[] nearbyOthers;
    RaycastHit[] raycastHits;
    Vector3 closeness;
    static readonly float avoidFactor = 1f;
    static readonly float avoidRadius = 0.5f;
    static readonly WaitForSeconds wait = new WaitForSeconds(1f);


    public SpaceTimePoint lastSeenPlayerPosition;
    public SpaceTimePoint lastHeardPlayerPosition;
    public SpaceTimePoint lastHeardDisturbancePosition;

    List<Transform> otherTransforms = new List<Transform>();
    readonly float PERCEPTION_INTERVAL = 0.025f;
    readonly float MAXIMUM_SIGHT_RANGE = 25f;
    bool awareOfCorpse;
    public Suspiciousness lastSuspicionLevel;


    public void Awake() {
        nearbyOthers = new Collider[32];
        navMeshPath = new NavMeshPath();
        stateMachine = new CivilianNPCBrain();
        motor = GetComponent<KinematicCharacterMotor>();
        motor.SimulatedCharacterMass = UnityEngine.Random.Range(25f, 2500f);

        Bind(sightCone.gameObject);
    }

    void Start() {
        gunHandler.Holster();
        StartCoroutine(Toolbox.RunJobRepeatedly(findNearby));
    }
    public void OnPoolActivate() {
        Awake();
        listener = gameObject.GetComponentInChildren<Listener>();
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

    public void Initialize() {
        EnterDefaultState();
    }

    public void ChangeState(CivilianNPCControlState routine) {
        stateMachine.ChangeState(routine);
    }
    void EnterDefaultState() {
        if (UnityEngine.Random.Range(0f, 1f) < 0.5f) {
            ChangeState(new LoiterState(this, characterController));
        } else {
            SocialGroup socialGroup = LookForSocialGroup();
            ChangeState(new CivilianSocializeState(this, characterController, speechTextController, socialGroup));
        }
    }
    public void StateFinished(CivilianNPCControlState routine) {
        switch (routine) {
            case CivilianPanicRunState:
            case CivilianCowerState:
            case CivilianReactToAttackState:
                if (UnityEngine.Random.Range(0f, 1f) < 0.1f) {
                    EnterDefaultState();
                }
                if (UnityEngine.Random.Range(0f, 1f) < 0.5f) {
                    ChangeState(new CivilianCowerState(this, characterController));
                } else {
                    ChangeState(new CivilianPanicRunState(this, characterController));
                }
                break;
            default:
                EnterDefaultState();
                break;
        }
    }

    SocialGroup LookForSocialGroup() {
        List<SocialGroup> socialGroups = GameObject.FindObjectsOfType<SocialGroup>()
            // .Where(socialGroup => area.bounds.Contains(socialGroup.transform.position))
            .Where(socialGroup => socialGroup.members.Count < 4)
            .ToList();
        if (socialGroups.Count > 0) {
            return Toolbox.RandomFromList(socialGroups);
        } else {
            Vector3 point = transform.position;
            NavMeshHit hit = new NavMeshHit();
            NavMeshQueryFilter filter = new NavMeshQueryFilter {
                areaMask = LayerUtil.KeySetToNavLayerMask(new HashSet<int>())
            };
            bool foundGoodPosition = NavMesh.SamplePosition(point, out hit, 10f, filter);
            if (!foundGoodPosition) {
                GameObject socialGroupObject = GameObject.Instantiate(Resources.Load("prefabs/socialGroup"), point, Quaternion.identity) as GameObject;
                SocialGroup socialGroup = socialGroupObject.GetComponent<SocialGroup>();
                return socialGroup;
            } else {
                GameObject socialGroupObject = GameObject.Instantiate(Resources.Load("prefabs/socialGroup"), hit.position, Quaternion.identity) as GameObject;
                SocialGroup socialGroup = socialGroupObject.GetComponent<SocialGroup>();
                return socialGroup;
            }
        }
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

        SetInputs(input);
        for (int i = 0; i < navMeshPath.corners.Length - 1; i++) {
            Debug.DrawLine(navMeshPath.corners[i], navMeshPath.corners[i + 1], Color.white);
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
                case LoiterState:
                case CivilianSocializeState:
                case CivilianCowerState:
                case CivilianPanicRunState:
                    alertHandler.ShowAlert(useWarnMaterial: true);
                    ChangeState(new CivilianReactToAttackState(this, speechTextController, noise, characterController));
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
                    case LoiterState:
                    case CivilianSocializeState:
                    case CivilianCowerState:
                    case CivilianPanicRunState:
                        alertHandler.ShowAlert(useWarnMaterial: true);
                        ChangeState(new CivilianReactToAttackState(this, speechTextController, noise, characterController));
                        break;
                }
            }
        }
    }

    // TODO: fix this
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
                if (bulletImpact.damage.hit.collider.transform.IsChildOf(transform)) {

                } else {
                    alertHandler.ShowAlert(useWarnMaterial: true);
                    SuspicionRecord record = SuspicionRecord.shotSuspicion();
                    GameManager.I.AddSuspicionRecord(record);
                    switch (stateMachine.currentState) {
                        case LoiterState:
                        case CivilianSocializeState:
                            ChangeState(new CivilianReactToAttackState(this, speechTextController, bulletImpact.damage, characterController));
                            break;
                    }
                }
            }
        }
    }

    void PerceivePlayerObject(Collider other, bool byPassVisibilityCheck = false) {
        float distance = Vector3.Distance(transform.position, other.bounds.center);
        if (byPassVisibilityCheck || GameManager.I.IsPlayerVisible(distance)) {
            lastSeenPlayerPosition = new SpaceTimePoint(other.bounds.center);
            Reaction reaction = ReactToPlayerSuspicion();
            Suspiciousness playerTotalSuspicion = GameManager.I.GetTotalSuspicion();
            if (playerTotalSuspicion >= Suspiciousness.suspicious) {
                alertHandler.ShowAlert(useWarnMaterial: true);
                if (GameManager.I.gameData.levelState.anyAlarmTerminalActivated() || playerTotalSuspicion >= Suspiciousness.aggressive) {
                    if (UnityEngine.Random.Range(0f, 1f) < 0.5f) {
                        ChangeState(new CivilianCowerState(this, characterController));
                    } else {
                        ChangeState(new CivilianPanicRunState(this, characterController));
                    }
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
        ChangeState(new CivilianReactToAttackState(this, speechTextController, damage, characterController));
        return DamageResult.NONE;
    }
}
