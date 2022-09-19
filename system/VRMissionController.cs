using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
public class VRMissionController : MonoBehaviour {
    enum State { normal, victory, fail }
    State state;
    VRMissionData data;
    public float NPCspawnTimer;
    public List<CharacterController> npcControllers = new List<CharacterController>();
    float startTime;
    public void StartVRMission(VRMissionData data) {
        Debug.Log("VRMissionController: start VR mission");
        this.data = data;
        CharacterController playerController = GameManager.I.playerObject.GetComponentInChildren<CharacterController>();
        playerController.OnCharacterDead += HandlePlayerDead;
        startTime = Time.time;
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
                GameManager.I.TransitionToState(GameState.inMenu);

                data.data.status = state switch {
                    State.victory => VRMissionMutableData.Status.victory,
                    State.fail => VRMissionMutableData.Status.fail,
                    _ => VRMissionMutableData.Status.inProgress
                };

                if (!SceneManager.GetSceneByName("VRMissionFinish").isLoaded) {
                    // SceneManager.LoadScene("VRMissionFinish", LoadSceneMode.Additive);
                    GameManager.I.LoadScene("VRMissionFinish", () => {
                        Debug.Log("loaded vr mission finish callback");
                        VRVictoryMenuController menuController = GameObject.FindObjectOfType<VRVictoryMenuController>();
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
        if (data.data.numberTotalNPCs < data.maxNumberNPCs && data.data.numberLiveNPCs < data.numberConcurrentNPCs) {
            NPCspawnTimer += Time.deltaTime;
            if (NPCspawnTimer > data.data.NPCspawnInterval) {
                NPCspawnTimer -= data.data.NPCspawnInterval;
                SpawnNPC();
            }
        } else {
            NPCspawnTimer = 0f;
        }
    }

    void SpawnNPC() {
        NPCSpawnPoint[] spawnPoints = GameObject.FindObjectsOfType<NPCSpawnPoint>().Where(spawn => !spawn.isStrikeTeamSpawn).ToArray();
        NPCSpawnPoint spawnPoint = Toolbox.RandomFromList(spawnPoints);
        GameObject npcObject = spawnPoint.SpawnNPC();
        CharacterController npcController = npcObject.GetComponentInChildren<CharacterController>();

        npcControllers.Add(npcController);
        npcController.OnCharacterDead += HandleNPCDead;
        data.data.numberTotalNPCs += 1;
        data.data.numberLiveNPCs += 1;
        Debug.Log($"spawn npc {data.data.numberLiveNPCs} {data.data.numberTotalNPCs} {data.data.numberNPCsKilled}");
    }

    void HandleNPCDead(CharacterController npc) {
        npc.OnCharacterDead -= HandleNPCDead;
        npcControllers.Remove(npc);
        data.data.numberLiveNPCs -= 1;
        data.data.numberNPCsKilled += 1;
        if (data.missionType == VRMissionType.hunt) {
            if (data.data.numberNPCsKilled >= data.maxNumberNPCs) {
                TransitionToState(State.victory);
            }
        }
    }
    void HandlePlayerDead(CharacterController npc) {
        TransitionToState(State.fail);
    }

    void SpawnAllNPCs() {
        foreach (NPCSpawnPoint spawnPoint in GameObject.FindObjectsOfType<NPCSpawnPoint>().Where(spawn => !spawn.isStrikeTeamSpawn)) {
            spawnPoint.SpawnNPC();
        }
    }

    void OnDestroy() {
        foreach (CharacterController npcController in npcControllers) {
            npcController.OnCharacterDead -= HandleNPCDead;
        }
        if (GameManager.I.playerObject != null) {
            CharacterController playerController = GameManager.I.playerObject.GetComponentInChildren<CharacterController>();
            playerController.OnCharacterDead -= HandlePlayerDead;
        }
    }
}
