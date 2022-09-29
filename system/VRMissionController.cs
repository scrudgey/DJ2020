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
    public void StartVRMission(VRMissionState state) {
        Debug.Log("VRMissionController: start VR mission");
        this.data = state;
        CharacterController playerController = GameManager.I.playerObject.GetComponentInChildren<CharacterController>();
        playerController.OnCharacterDead += HandlePlayerDead;

        if (!state.template.alarmHQEnabled) {

        }
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
        if (data.data.numberTotalNPCs < data.template.maxNumberNPCs && data.data.numberLiveNPCs < data.template.numberConcurrentNPCs) {
            NPCspawnTimer += Time.deltaTime;
            if (NPCspawnTimer > data.template.NPCspawnInterval) {
                NPCspawnTimer -= data.template.NPCspawnInterval;
                SpawnNPC();
            }
        } else {
            NPCspawnTimer = 0f;
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
        if (data.template.missionType == VRMissionType.hunt) {
            if (data.data.numberNPCsKilled >= data.template.maxNumberNPCs) {
                TransitionToState(State.victory);
            }
        }
    }
    void HandlePlayerDead(CharacterController npc) {
        TransitionToState(State.fail);
    }

    void SpawnAllNPCs() {
        foreach (NPCSpawnPoint spawnPoint in GameObject.FindObjectsOfType<NPCSpawnPoint>().Where(spawn => !spawn.isStrikeTeamSpawn)) {
            spawnPoint.SpawnNPC(data.template.npc1State);
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
