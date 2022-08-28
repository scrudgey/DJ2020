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
    public Dictionary<GameObject, HQReport> reports;
    float clearCaptionTimer;
    float alarmShutdownTimer;

    public void ActivateAlarm() {
        if (!gameData.levelData.alarm) {
            alarmSoundTimer = alarmSoundInterval;
        }
        gameData.levelData.alarm = true;
        gameData.levelData.alarmCountDown = 30f;
        OnSuspicionChange?.Invoke();
        alarmShutdownTimer = 0f;
    }
    public void DeactivateAlarm() {
        gameData.levelData.alarm = false;
        gameData.levelData.alarmCountDown = 0f;
        strikeTeamResponseTimer = 0f;
        OnSuspicionChange?.Invoke();
        alarmShutdownTimer = 0f;
    }
    public void OpenReportTicket(GameObject reporter, HQReport report) {
        if (!gameData.levelData.hasHQ)
            return;
        if (reports == null) reports = new Dictionary<GameObject, HQReport>();
        if (reports.ContainsKey(reporter)) {
            ContactReportTicket(reporter);
        } else {
            report.timeOfLastContact = Time.time;
            reports[reporter] = report;
        }

    }
    public bool ContactReportTicket(GameObject reporter) {
        if (!gameData.levelData.hasHQ)
            return false;
        if (reports.ContainsKey(reporter)) {
            HQReport report = reports[reporter];
            report.timeOfLastContact = Time.time;
            report.timer += Time.deltaTime;
            if (report.timer > report.lifetime) {
                CloseReport(new KeyValuePair<GameObject, HQReport>(reporter, report));
                return true;
            } else return false;
        } else {
            Debug.LogWarning("reporter contacting HQ without opening ticket");
            return false;
        }
    }
    void UpdateReportTickets() {
        if (!gameData.levelData.hasHQ)
            return;
        if (reports == null) reports = new Dictionary<GameObject, HQReport>();
        foreach (KeyValuePair<GameObject, HQReport> kvp in reports) {
            if (Time.time - kvp.Value.timeOfLastContact > 10) {
                TimeOutReport(kvp);
            }
        }
        if (clearCaptionTimer > 0) {
            clearCaptionTimer -= Time.deltaTime;
            if (clearCaptionTimer <= 0) {
                OnCaptionChange("");
            }
        }
    }
    void CloseReport(KeyValuePair<GameObject, HQReport> kvp) {
        locationOfLastDisturbance = kvp.Value.locationOfLastDisturbance;
        if (kvp.Value.desiredAlarmState) {
            ActivateAlarm();
            DisplayHQResponse("HQ: Understood. Dispatching strike team.");
        } else {
            DeactivateAlarm();
            DisplayHQResponse("HQ: Understood. Disabling alarm.");
        }
        reports.Remove(kvp.Key);
    }
    void InitiateAlarmShutdown() {
        if (alarmShutdownTimer > 0f)
            return;
        alarmShutdownTimer = 5f;
        List<SphereRobotAI> ais = new List<SphereRobotAI>(GameObject.FindObjectsOfType<SphereRobotAI>());
        if (ais.Count == 0) {
            DeactivateAlarm();
        } else {
            SphereRobotAI ai = Toolbox.RandomFromList(ais);
            SpeechTextController speechTextController = ai.GetComponentInChildren<SpeechTextController>();
            ai.ChangeState(new DisableAlarmState(ai, speechTextController));
        }
    }
    void TimeOutReport(KeyValuePair<GameObject, HQReport> kvp) {
        DisplayHQResponse("HQ: What's going on? Respond!");
        ActivateAlarm();
        reports.Remove(kvp.Key);
    }
    void DisplayHQResponse(String response) {
        OnCaptionChange(response);
        clearCaptionTimer = 5f;
    }
    public void UpdateAlarm() {
        if (gameData.levelData.alarmCountDown > 0) {
            gameData.levelData.alarmCountDown -= Time.deltaTime;
            if (gameData.levelData.alarmCountDown <= 0) {
                InitiateAlarmShutdown();
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
        if (alarmShutdownTimer > 0f) {
            alarmShutdownTimer -= Time.deltaTime;
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