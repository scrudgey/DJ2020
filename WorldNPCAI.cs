using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KinematicCharacterController;
using UnityEngine;
using UnityEngine.AI;

public class WorldNPCAI : MonoBehaviour {
    public NavMeshPath navMeshPath;
    public CharacterController characterController;
    public GunHandler gunHandler;
    public WorldNPCBrain stateMachine;
    public KinematicCharacterMotor motor;
    public SpeechTextController speechTextController;

    Collider[] nearbyOthers;
    Vector3 closeness;
    static readonly float avoidFactor = 1f;
    static readonly float avoidRadius = 0.5f;
    static readonly WaitForSeconds wait = new WaitForSeconds(1f);

    List<Transform> otherTransforms = new List<Transform>();
    public void Awake() {
        nearbyOthers = new Collider[32];
        navMeshPath = new NavMeshPath();
        stateMachine = new WorldNPCBrain();
        motor = GetComponent<KinematicCharacterMotor>();
        motor.SimulatedCharacterMass = UnityEngine.Random.Range(25f, 2500f);
    }

    void Start() {
        gunHandler.Holster();
        StartCoroutine(Toolbox.RunJobRepeatedly(findNearby));
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
            WorldNPCAI otherAI = collider.GetComponent<WorldNPCAI>();
            if (otherAI != null) {
                otherTransforms.Add(otherAI.transform);
            }
        }
    }

    public void Initialize(WalkToStoreState.StoreType storeType) {
        EnterDefaultState(storeType);
    }

    void EnterDefaultState(WalkToStoreState.StoreType storeType) {
        if (UnityEngine.Random.Range(0f, 1f) < 0.05f || storeType == WalkToStoreState.StoreType.none) {
            WalkToRandomDestination();
        } else {
            SocialGroup socialGroup = LookForSocialGroup(storeType);
            ChangeState(new SocializeState(this, characterController, speechTextController, socialGroup, storeType));
        }
    }

    void WalkToRandomDestination() {
        var x = Enum.GetValues(typeof(WalkToStoreState.StoreType)).Cast<WalkToStoreState.StoreType>()
                                        .ToList();
        x.Remove(WalkToStoreState.StoreType.none);
        x.Add(WalkToStoreState.StoreType.alley);
        x.Add(WalkToStoreState.StoreType.bar);
        x.Add(WalkToStoreState.StoreType.item);
        WalkToStoreState.StoreType destination =
            Toolbox.RandomFromList<WalkToStoreState.StoreType>(x);
        ChangeState(new WalkToStoreState(this, characterController, destination));
    }

    public void ChangeState(WorldNPCControlState routine) {
        stateMachine.ChangeState(routine);
    }

    public void StateFinished(WorldNPCControlState routine) {
        switch (routine) {
            case WalkToStoreState walkState:
                SocialGroup socialGroup = LookForSocialGroup(walkState.destinationStore);
                ChangeState(new SocializeState(this, characterController, speechTextController, socialGroup, walkState.destinationStore));
                break;
            case SocializeState socializeState:
                if (socializeState.location == WalkToStoreState.StoreType.bar) {
                    if (UnityEngine.Random.Range(0f, 1f) < 0.5f) {
                        ChangeState(new LingerInBarState(this, characterController));
                    } else {
                        WalkToRandomDestination();
                    }
                } else {
                    WalkToRandomDestination();
                }
                break;
            default:
                WalkToRandomDestination();
                break;
        }
    }

    SocialGroup LookForSocialGroup(WalkToStoreState.StoreType destination) {

        GameObject destinationContainer = destination switch {
            WalkToStoreState.StoreType.bar => GameObject.Find("barDestination"),
            WalkToStoreState.StoreType.item => GameObject.Find("itemDestination"),
            WalkToStoreState.StoreType.gun => GameObject.Find("gunDestination"),
            WalkToStoreState.StoreType.alley => GameObject.Find("alleyDestination"),
        };
        BoxCollider area = destinationContainer.GetComponent<BoxCollider>();

        List<SocialGroup> socialGroups = GameObject.FindObjectsOfType<SocialGroup>()
            .Where(socialGroup => area.bounds.Contains(socialGroup.transform.position))
            .Where(socialGroup => socialGroup.members.Count < 4)
            .ToList();
        // Debug.Log($"[social] {destination} found social groups: {socialGroups.Count} for {destinationContainer}");
        if (socialGroups.Count > 0) {
            return Toolbox.RandomFromList(socialGroups);
        } else {
            Vector3 point = Toolbox.RandomInsideBounds(area);
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

}
