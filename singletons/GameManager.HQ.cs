using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KinematicCharacterController;
using UnityEngine;
public partial class GameManager : Singleton<GameManager> {
    public AudioClip alarmSound;
    public float alarmSoundTimer;
    public float alarmSoundInterval;
    int strikeTeamCount = 0;
    float strikeTeamSpawnInterval = 0.5f;
    float strikeTeamSpawnTimer = 0f;
    float strikeTeamResponseTimer = 0f;

    public void ActivateAlarm() {
        gameData.levelData.alarm = true;
        gameData.levelData.alarmCountDown = 60f;
        OnSuspicionChange?.Invoke();
        alarmSoundTimer = alarmSoundInterval;
    }
    public void DeactivateAlarm() {
        gameData.levelData.alarm = false;
        gameData.levelData.alarmCountDown = 0f;
        strikeTeamResponseTimer = 0f;
        OnSuspicionChange?.Invoke();
    }
    public void ReportToHQ(bool activateAlarm) {
        if (!gameData.levelData.hasHQ)
            return;
        if (activateAlarm) {
            ActivateAlarm();
        } else {
            DeactivateAlarm();
        }
    }
    public void UpdateAlarm() {
        if (gameData.levelData.alarmCountDown > 0) {
            gameData.levelData.alarmCountDown -= Time.deltaTime;
            if (gameData.levelData.alarmCountDown <= 0) {
                DeactivateAlarm();
            }
        }

        if (gameData.levelData.alarm) {
            if (strikeTeamSpawnPoint != null) { // TODO: check level data 
                UpdateStrikeTeamSpawn();
            }

            alarmSoundTimer += Time.deltaTime;
            if (alarmSoundTimer > alarmSoundInterval) {
                Debug.Log($"{alarmSoundTimer} {alarmSoundInterval}");
                alarmSoundTimer -= alarmSoundInterval;
                audioSource.PlayOneShot(alarmSound);
            }
        }
    }

    void UpdateStrikeTeamSpawn() {
        if (strikeTeamCount < gameData.levelData.strikeTeamMaxSize) {
            if (strikeTeamResponseTimer < gameData.levelData.strikeTeamResponseTime) {
                strikeTeamResponseTimer += Time.deltaTime;
            } else {
                strikeTeamSpawnTimer += Time.deltaTime;
                if (strikeTeamSpawnTimer > strikeTeamSpawnInterval) {
                    strikeTeamSpawnTimer -= strikeTeamSpawnInterval;
                    SpawnStrikeTeamMember(strikeTeamSpawnPoint.position);
                }
            }
        }
    }

    void SpawnStrikeTeamMember(Vector3 position) {
        GameObject npc = PoolManager.I.GetPool("prefabs/NPC").GetObject(position);
        CharacterCamera characterCamera = GameObject.FindObjectOfType<CharacterCamera>();
        PatrolRoute patrolRoute = GameObject.FindObjectOfType<PatrolRoute>();

        CharacterController controller = npc.GetComponentInChildren<CharacterController>();
        SphereRobotAI ai = npc.GetComponentInChildren<SphereRobotAI>();
        KinematicCharacterMotor motor = npc.GetComponentInChildren<KinematicCharacterMotor>();
        controller.OrbitCamera = characterCamera;
        ai.patrolRoute = patrolRoute;
        motor.SetPosition(position, bypassInterpolation: true);

        strikeTeamCount += 1;
    }
}