using AI;
using UnityEngine;
using UnityEngine.AI;
public class WalkToStoreState : WorldNPCControlState {
    public static readonly string NAV_POINT_KEY = "nav_point_key";

    public enum StoreType { bar, item, gun, alley }
    public StoreType destinationStore;
    private TaskNode rootTaskNode;
    WorldNPCAI ai;
    CharacterController characterController;
    public WalkToStoreState(WorldNPCAI ai, CharacterController characterController, StoreType destinationStore) : base(ai) {
        this.ai = ai;
        this.characterController = characterController;
        this.destinationStore = destinationStore;
    }
    public override void Enter() {
        base.Enter();
        SetupRootNode();
    }
    void SetupRootNode() {
        Vector3 destination = GetDestination(destinationStore);
        rootTaskNode =
        new Sequence(
         new TaskMoveToKey(owner.transform, NAV_POINT_KEY, new System.Collections.Generic.HashSet<int>(), characterController, arrivalDistance: 0.5f) {
             speedCoefficient = 0.5f
         },
         new TaskTimerDectorator(Random.Range(1f, 3f))
        );
        rootTaskNode.SetData(NAV_POINT_KEY, destination);
        // rootTaskNode = new TaskPatrol(owner.transform, patrolRoute, patrolType, owner.physicalKeys, characterController);
    }

    Vector3 GetDestination(StoreType destination) {
        GameObject destinationContainer = destination switch {
            StoreType.bar => GameObject.Find("barDestination"),
            StoreType.item => GameObject.Find("itemDestination"),
            StoreType.gun => GameObject.Find("gunDestination"),
            StoreType.alley => GameObject.Find("alleyDestination"),
        };
        BoxCollider area = destinationContainer.GetComponent<BoxCollider>();
        return Toolbox.RandomInsideBounds(area);
    }

    public override PlayerInput Update(ref PlayerInput input) {
        TaskState result = rootTaskNode.Evaluate(ref input);
        if (result == TaskState.success) {
            owner.StateFinished(this);
        }
        return input;
    }

}