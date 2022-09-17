using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class VRMissionController : MonoBehaviour {
    VRMissionData data;
    public int numberTotalNPCs;
    public int numberLiveNPCs;
    public float NPCspawnInterval = 1f;
    public float NPCspawnTimer;
    public List<CharacterController> npcControllers = new List<CharacterController>();

    public void StartVRMission(VRMissionData data) {
        Debug.Log("start VR mission");
        this.data = data;
        CharacterController playerController = GameManager.I.playerObject.GetComponentInChildren<CharacterController>();
        playerController.OnCharacterDead += HandlePlayerDead;
    }

    void Update() {
        if (numberTotalNPCs < data.numberNPCs && numberLiveNPCs < 3) {
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
        npcController.OnCharacterDead += HandleNPCDead;
        npcControllers.Add(npcController);
        numberTotalNPCs += 1;
        numberLiveNPCs += 1;
        Debug.Log($"spawn npc {numberLiveNPCs} {numberTotalNPCs}");
    }

    void HandleNPCDead() {
        numberLiveNPCs -= 1;
        Debug.Log($"NPCdead {numberLiveNPCs} {numberTotalNPCs}");
    }
    void HandlePlayerDead() {
        Debug.Log("player dead");
    }

    void SpawnAllNPCs() {
        foreach (NPCSpawnPoint spawnPoint in GameObject.FindObjectsOfType<NPCSpawnPoint>().Where(spawn => !spawn.isStrikeTeamSpawn)) {
            Debug.Log($"spawning {spawnPoint}");
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
