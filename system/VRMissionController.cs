using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class VRMissionController : MonoBehaviour {
    enum State { normal, victory, fail }
    State state;
    VRMissionData data;
    public int numberTotalNPCs;
    public int numberLiveNPCs;
    public int numberNPCsKilled;
    public float NPCspawnInterval = 10f;
    public float NPCspawnTimer;
    public List<CharacterController> npcControllers = new List<CharacterController>();
    void TransitionToState(State newState) {
        State tmpInitialState = state;
        OnStateExit(tmpInitialState, newState);
        state = newState;
        OnStateEnter(newState, tmpInitialState);
    }
    void OnStateEnter(State state, State fromState) {
        Debug.Log($"VR mission entering state {state} from {fromState}");
        switch (state) {
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
    public void StartVRMission(VRMissionData data) {
        Debug.Log("start VR mission");
        this.data = data;
        CharacterController playerController = GameManager.I.playerObject.GetComponentInChildren<CharacterController>();
        playerController.OnCharacterDead += HandlePlayerDead;
    }

    void Update() {
        if (numberTotalNPCs < data.maxNumberNPCs && numberLiveNPCs < data.numberConcurrentNPCs) {
            NPCspawnTimer += Time.deltaTime;
            if (NPCspawnTimer > NPCspawnInterval) {
                NPCspawnTimer -= NPCspawnInterval;
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
        numberTotalNPCs += 1;
        numberLiveNPCs += 1;
        Debug.Log($"spawn npc {numberLiveNPCs} {numberTotalNPCs} {numberNPCsKilled}");
    }

    void HandleNPCDead(CharacterController npc) {
        npc.OnCharacterDead -= HandleNPCDead;
        npcControllers.Remove(npc);
        numberLiveNPCs -= 1;
        numberNPCsKilled += 1;
        if (data.missionType == VRMissionType.hunt) {
            if (numberNPCsKilled >= data.maxNumberNPCs) {
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
        CharacterController playerController = GameManager.I.playerObject.GetComponentInChildren<CharacterController>();
        playerController.OnCharacterDead -= HandlePlayerDead;
    }
}
