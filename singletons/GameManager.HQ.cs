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

    public void ActivateHQRadio() {
        AlarmHQTerminal terminal = levelHQTerminal();
        if (terminal != null) {
            AlarmNode node = GetAlarmNode(terminal.idn);
            node.alarmTriggered = true;
            node.countdownTimer = 30f;
            RefreshAlarmGraph();
            SetLevelAlarmActive();
        }
    }
    public void SetLevelAlarmActive() {
        if (!gameData.levelState.anyAlarmActive()) {
            alarmSoundTimer = alarmSoundInterval;
        }
        OnSuspicionChange?.Invoke();
    }
    public bool isAlarmRadioInProgress(GameObject exclude) {
        foreach (HQReport report in reports.Values) {
            if (report.desiredAlarmState && report.reporter != exclude) {
                return true;
            }
        }
        return false;
    }
    public AlarmHQTerminal levelHQTerminal() => alarmComponents.Values
            .Where(node => node != null && node is AlarmHQTerminal)
            .Select(component => (AlarmHQTerminal)component)
            .First();

    public void DeactivateAlarm() {
        if (applicationIsQuitting)
            return;
        strikeTeamResponseTimer = 0f;
        OnSuspicionChange?.Invoke();

        // reset strike team 
        try {
            PrefabPool pool = PoolManager.I.GetPool("prefabs/NPC");
            gameData.levelState.template.strikeTeamMaxSize = Math.Min(3, pool.objectsInPool.Count);
        }
        finally {

        }
        strikeTeamCount = 0;
    }
    public void OpenReportTicket(GameObject reporter, HQReport report) {
        if (levelHQTerminal() != null) {
            if (reports == null) reports = new Dictionary<GameObject, HQReport>();
            if (reports.ContainsKey(reporter)) {
                ContactReportTicket(reporter);
            } else {
                report.timeOfLastContact = Time.time;
                reports[reporter] = report;
            }
        }
    }
    public bool ContactReportTicket(GameObject reporter) {
        if (levelHQTerminal() != null) {
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
        return true;
    }
    void UpdateReportTickets() {
        if (reports == null) reports = new Dictionary<GameObject, HQReport>();
        List<GameObject> timedOutReports = new List<GameObject>();
        foreach (KeyValuePair<GameObject, HQReport> kvp in reports) {
            if (Time.time - kvp.Value.timeOfLastContact > 10) {
                TimeOutReport(kvp);
                timedOutReports.Add(kvp.Key);
            }
        }
        foreach (GameObject key in timedOutReports) {
            reports.Remove(key);
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
            ActivateHQRadio();
            DisplayHQResponse("HQ: Understood. Dispatching strike team.");
        } else {
            DeactivateAlarm();
            DisplayHQResponse("HQ: Understood. Disabling alarm.");
        }
        reports.Remove(kvp.Key);
    }
    void InitiateAlarmShutdown() {
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
        ActivateHQRadio();
    }
    void DisplayHQResponse(String response) {
        OnCaptionChange(response);
        clearCaptionTimer = 5f;
    }
    public float alarmCountdown() {
        float timer = 0f;
        foreach (AlarmNode node in gameData.levelState.delta.alarmGraph.nodes.Values) {
            timer = Math.Max(timer, node.countdownTimer);
        }
        return timer;
    }
    public void UpdateGraphs() {
        float alarmTimerOrig = alarmCountdown();

        gameData?.levelState?.delta.alarmGraph?.Update();

        float alarmTimer = alarmCountdown();
        if (alarmTimer <= 0 && alarmTimerOrig > 0) {
            InitiateAlarmShutdown();
        }
    }
    public void UpdateAlarm() {
        if (gameData.levelState.anyAlarmActive()) {
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
        if (strikeTeamCount < gameData.levelState.template.strikeTeamMaxSize) {
            if (strikeTeamResponseTimer < gameData.levelState.template.strikeTeamResponseTime) {
                strikeTeamResponseTimer += Time.deltaTime;
            } else {
                strikeTeamSpawnTimer += Time.deltaTime;
                if (strikeTeamSpawnTimer > strikeTeamSpawnInterval) {
                    strikeTeamSpawnTimer -= strikeTeamSpawnInterval;
                    SpawnStrikeTeamMember();
                }
            }
        }
    }

    void SpawnStrikeTeamMember() {
        GameObject npc = strikeTeamSpawnPoint.SpawnNPC(gameData.levelState.template.strikeTeamTemplate);
        SphereRobotAI ai = npc.GetComponentInChildren<SphereRobotAI>();

        if (strikeTeamCount == 0) {
            ai.ChangeState(new SearchDirectionState(ai, locationOfLastDisturbance, doIntro: false, speedCoefficient: 1.5f));
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