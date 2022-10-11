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
    public List<CharacterController> npcControllers = new List<CharacterController>();
    float startTime;
    ActionLogHandler actionLogHandler;
    List<VRDataStore> dataStores;
    VRDataStore targetDatastore;
    Stack<VRDataStore> datastoreStack;
    VRStatHandler vRStatHandler;
    public void StartVRMission(VRMissionState state) {
        Debug.Log("VRMissionController: start VR mission");
        this.data = state;
        CharacterController playerController = GameManager.I.playerObject.GetComponentInChildren<CharacterController>();
        playerController.OnCharacterDead += HandlePlayerDead;
        int NPCPoolSize = state.template.numberConcurrentNPCs + GameManager.I.gameData.levelState.delta.strikeTeamMaxSize;
        // Debug.Log($"initializing NPC pool with size {NPCPoolSize}");
        PoolManager.I.RegisterPool("prefabs/NPC", poolSize: NPCPoolSize);
        if (!state.template.alarmHQEnabled) {
            AlarmHQTerminal levelHQTerminal = GameManager.I.levelHQTerminal();
            if (levelHQTerminal != null) {
                GameManager.I.RemoveAlarmNode(levelHQTerminal.idn);
            }
        }
        startTime = Time.time;
        if (state.template.missionType == VRMissionType.steal) {
            InitializeStealDataMission();
        }

    }
    void InitializeStealDataMission() {
        Debug.Log("initialize steal data mission");
        dataStores = new List<VRDataStore>();
        foreach (VRDataStore dataStore in GameObject.FindObjectsOfType<VRDataStore>()) {
            Debug.Log(dataStore);
            dataStores.Add(dataStore);
            dataStore.OnDataStoreOpened += HandleVRDataStoreOpened;
        }
        datastoreStack = new Stack<VRDataStore>();
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
        // Debug.Log($"new target datastore: {targetDatastore}");
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
            // Debug.Log($"datastores opened: {numberDataStoresOpened}");
        }
    }
    void SendLogMessage(string message) {
        if (actionLogHandler == null)
            actionLogHandler = GameObject.FindObjectOfType<ActionLogHandler>();
        actionLogHandler.ShowMessage(message);
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
                // GameManager.I.TransitionToInputMode(InputMode.none);
                data.data.secondsPlayed = Time.time - startTime;
                GameManager.I.TransitionToState(GameState.inMenu);

                data.data.status = state switch {
                    State.victory => VRMissionDelta.Status.victory,
                    State.fail => VRMissionDelta.Status.fail,
                    _ => VRMissionDelta.Status.inProgress
                };

                if (!SceneManager.GetSceneByName("VRMissionFinish").isLoaded) {
                    // SceneManager.LoadScene("VRMissionFinish", LoadSceneMode.Additive);
                    GameManager.I.LoadScene("VRMissionFinish", () => {
                        Debug.Log("loaded vr mission finish callback");
                        VRMissionVictoryMenuController menuController = GameObject.FindObjectOfType<VRMissionVictoryMenuController>();
                        menuController.Initialize(data);
                    }, unloadAll: false);
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
        if (vRStatHandler == null) {
            UIController uiController = GameObject.FindObjectOfType<UIController>();
            if (uiController != null) {
                uiController.ShowVRStats();
                vRStatHandler = uiController.vRStatHandler;
            }
        } else {
            vRStatHandler.SetDisplay(data);
        }
    }

    void SpawnNPC() {
        NPCSpawnPoint[] spawnPoints = GameObject.FindObjectsOfType<NPCSpawnPoint>().Where(spawn => !spawn.isStrikeTeamSpawn).ToArray();
        NPCSpawnPoint spawnPoint = Toolbox.RandomFromList(spawnPoints);

        GameObject npcObject = spawnPoint.SpawnNPC(data.template.npc1State);
        CharacterController npcController = npcObject.GetComponentInChildren<CharacterController>();

        npcControllers.Add(npcController);
        npcController.OnCharacterDead += HandleNPCDead;
        data.data.numberTotalNPCs += 1;
        data.data.numberLiveNPCs += 1;
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