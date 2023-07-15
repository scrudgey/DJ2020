using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
public class VRMissionController : MonoBehaviour {
    enum State { normal, victory, fail }
    State state;
    VRMissionState data;
    public float NPCspawnTimer;
    public List<CharacterController> npcControllers;
    float startTime;
    ActionLogHandler actionLogHandler;
    List<VRDataStore> dataStores;
    VRDataStore targetDatastore;
    Stack<VRDataStore> datastoreStack;
    VRStatHandler vRStatHandler;
    NPCSpawnPoint[] spawnPoints;

    public void StartVRMission(VRMissionState state) {
        Debug.Log("VRMissionController: start VR mission");
        this.data = state;
        npcControllers = new List<CharacterController>();
        dataStores = new List<VRDataStore>();
        datastoreStack = new Stack<VRDataStore>();

        CharacterController playerController = GameManager.I.playerObject.GetComponentInChildren<CharacterController>();
        playerController.OnCharacterDead += HandlePlayerDead;
        int NPCPoolSize = state.template.numberConcurrentNPCs + GameManager.I.gameData.levelState.template.strikeTeamMaxSize;
        // Debug.Log($"initializing NPC pool with size {NPCPoolSize}");
        PoolManager.I.RegisterPool("prefabs/NPC", poolSize: NPCPoolSize);
        spawnPoints = GameObject.FindObjectsOfType<NPCSpawnPoint>().Where(spawn => !spawn.isStrikeTeamSpawn).ToArray();
        if (!state.template.alarmHQEnabled) {
            AlarmRadio levelHQTerminal = GameManager.I.levelRadioTerminal();
            if (levelHQTerminal != null) {
                GameManager.I.RemoveAlarmNode(levelHQTerminal.idn);
            }
        }
        startTime = Time.time;
        if (state.template.missionType == VRMissionType.steal) {
            InitializeStealDataMission();
        }
        GameManager.I.OnNPCSpawn += HandleStrikeTeamSpawn;
    }
    void InitializeStealDataMission() {
        Debug.Log("initialize steal data mission");
        foreach (VRDataStore dataStore in GameObject.FindObjectsOfType<VRDataStore>()) {
            // Debug.Log(dataStore);
            dataStores.Add(dataStore);
            dataStore.OnDataStoreOpened += HandleVRDataStoreOpened;
        }
        GameObject.FindObjectsOfType<VRDataStore>().OrderBy(a => Guid.NewGuid()).ToList().ForEach(datastoreStack.Push);
        SetTargetDataStore();
    }
    void SetTargetDataStore() {
        if (datastoreStack.Count <= 0) {
            datastoreStack = new Stack<VRDataStore>();
            GameObject.FindObjectsOfType<VRDataStore>().OrderBy(a => Guid.NewGuid()).ToList().ForEach(datastoreStack.Push);
        }
        targetDatastore = datastoreStack.Pop();
        targetDatastore.ActivateCallout();
    }
    void HandleVRDataStoreOpened(VRDataStore target) {
        if (target == targetDatastore) {
            target.PlayParticles();
            target.DeactivateCallout();
            data.data.numberDataStoresOpened += 1;
            SendLogMessage($"Data stolen: {data.data.numberDataStoresOpened} / {data.template.targetDataCount}");
            if (data.data.numberDataStoresOpened >= data.template.targetDataCount) {
                TransitionToState(State.victory);
            }
            SetTargetDataStore();
        }
    }
    void SendLogMessage(string message) {
        // if (actionLogHandler == null)
        //     actionLogHandler = GameObject.FindObjectOfType<ActionLogHandler>();
        // actionLogHandler.ShowMessage(message);
    }
    void TransitionToState(State newState) {
        State tmpInitialState = state;
        OnStateExit(tmpInitialState, newState);
        state = newState;
        OnStateEnter(newState, tmpInitialState);
    }
    void OnStateEnter(State state, State fromState) {
        Debug.Log($"VR mission entering state {state} from {fromState}");
        switch (state) {
            case State.fail:
            case State.victory:
                data.data.secondsPlayed = Time.time - startTime;
                data.data.status = state switch {
                    State.victory => VRMissionDelta.Status.victory,
                    State.fail => VRMissionDelta.Status.fail,
                    _ => VRMissionDelta.Status.inProgress
                };

                if (GameManager.I.activeMenuType != MenuType.VRMissionFinish) {
                    GameManager.I.ShowMenu(MenuType.VRMissionFinish, () => {
                        VRMissionVictoryMenuController menuController = GameObject.FindObjectOfType<VRMissionVictoryMenuController>();
                        menuController.Initialize(data);
                    });
                }
                break;
            default:
                break;
        }
    }
    void OnStateExit(State state, State toState) {
        switch (state) {
            default:
                break;
        }
    }


    void Update() {
        data.data.secondsPlayed = Time.time - startTime;
        if (data.template.missionType == VRMissionType.time) {
            if (data.data.secondsPlayed > data.template.timeLimit && state != State.fail) {
                SendLogMessage($"Time limit!");
                TransitionToState(State.fail);
            }
        }
        if (data.data.numberLiveNPCs < data.template.numberConcurrentNPCs) {
            if ((data.template.missionType == VRMissionType.combat && data.data.numberTotalNPCs < data.template.maxNumberNPCs) ||
                (data.template.missionType == VRMissionType.steal) ||
                (data.template.missionType == VRMissionType.time)) {
                NPCspawnTimer += Time.deltaTime;
                if (NPCspawnTimer > data.template.NPCspawnInterval) {
                    NPCspawnTimer -= data.template.NPCspawnInterval;
                    SpawnNPC();
                }
            } else {
                NPCspawnTimer = 0f;
            }
        }

        // TODO: fix  this ugly hack!!
        // it could be done via binding VR stat handler to VR mission controller on VR mission start
        if (vRStatHandler == null) {
            if (GameManager.I.uiController != null) {
                GameManager.I.uiController.ShowVRStats();
                vRStatHandler = GameManager.I.uiController.vRStatHandler;
            }
        } else {
            vRStatHandler.SetDisplay(data);
        }
    }
    void HandleStrikeTeamSpawn(GameObject npc) {
        CharacterController npcController = npc.GetComponentInChildren<CharacterController>();
        npcControllers.Add(npcController);
        npcController.OnCharacterDead += HandleStrikeTeamMemberDead;
    }
    void SpawnNPC() {
        NPCSpawnPoint spawnPoint = Toolbox.RandomFromList(spawnPoints);
        GameObject npcObject = spawnPoint.SpawnNPC(data.template.npc1State);
        CharacterController npcController = npcObject.GetComponentInChildren<CharacterController>();
        npcControllers.Add(npcController);
        npcController.OnCharacterDead += HandleNPCDead;
        data.data.numberTotalNPCs += 1;
        data.data.numberLiveNPCs += 1;
    }

    void HandleStrikeTeamMemberDead(CharacterController npc) {
        npc.OnCharacterDead -= HandleStrikeTeamMemberDead;
        npcControllers.Remove(npc);
        data.data.numberNPCsKilled += 1;
        if (data.template.missionType == VRMissionType.combat || data.template.missionType == VRMissionType.time) {
            SendLogMessage($"Kill count: {data.data.numberNPCsKilled} / {data.template.maxNumberNPCs}");
            if (data.data.numberNPCsKilled >= data.template.maxNumberNPCs) {
                TransitionToState(State.victory);
            }
        }
    }
    void HandleNPCDead(CharacterController npc) {
        npc.OnCharacterDead -= HandleNPCDead;
        npcControllers.Remove(npc);
        data.data.numberLiveNPCs -= 1;
        data.data.numberNPCsKilled += 1;
        if (data.template.missionType == VRMissionType.combat || data.template.missionType == VRMissionType.time) {
            SendLogMessage($"Kill count: {data.data.numberNPCsKilled} / {data.template.maxNumberNPCs}");
            if (data.data.numberNPCsKilled >= data.template.maxNumberNPCs) {
                TransitionToState(State.victory);
            }
        }
    }
    void HandlePlayerDead(CharacterController npc) {
        TransitionToState(State.fail);
    }

    void OnDestroy() {
        if (GameManager.I != null)
            GameManager.I.OnNPCSpawn -= HandleStrikeTeamSpawn;
        foreach (CharacterController npcController in npcControllers) {
            npcController.OnCharacterDead -= HandleNPCDead;
        }
        foreach (VRDataStore dataStore in dataStores) {
            dataStore.OnDataStoreOpened -= HandleVRDataStoreOpened;
        }
        if (GameManager.I.playerObject != null) {
            CharacterController playerController = GameManager.I.playerObject.GetComponentInChildren<CharacterController>();
            playerController.OnCharacterDead -= HandlePlayerDead;
        }
    }
}
