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
    Vector3 locationOfLastDisturbance;
    public GameObject lastStrikeTeamMember;

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
    public void ReportToHQ(bool activateAlarm, Vector3 disturbancePosition) {
        if (!gameData.levelData.hasHQ)
            return;
        locationOfLastDisturbance = disturbancePosition;
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
        ai.overrideDefaultState = true;
        ai.patrolRoute = patrolRoute;
        motor.SetPosition(position, bypassInterpolation: true);

        if (strikeTeamCount == 0) {
            ai.ChangeState(new SearchDirectionState(ai, locationOfLastDisturbance, doIntro: false));
            lastStrikeTeamMember = npc;
        } else if (strikeTeamCount == 1) {
            ai.ChangeState(new FollowTheLeaderState(ai, lastStrikeTeamMember, headBehavior: AI.TaskFollowTarget.HeadBehavior.right));
            lastStrikeTeamMember = npc;
        } else if (strikeTeamCount == 2) {
            ai.ChangeState(new FollowTheLeaderState(ai, lastStrikeTeamMember, headBehavior: AI.TaskFollowTarget.HeadBehavior.left));
        }
        strikeTeamCount += 1;
    }
}