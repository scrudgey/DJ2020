using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KinematicCharacterController;
using UnityEngine;
public partial class GameManager : Singleton<GameManager> {

    public enum GuardPhase { normal, strikeTeam }
    public float alarmSoundTimer;
    public float alarmSoundInterval;
    Vector3 locationOfLastDisturbance;
    public GameObject lastStrikeTeamMember;
    // can't be part of level state: not serializable?
    public Dictionary<GameObject, HQReport> reports;
    float clearCaptionTimer;
    public Action<GameObject> OnNPCSpawn;

    public void ActivateHQRadioNode() {
        AlarmRadio terminal = levelRadioTerminal();
        if (terminal != null) {
            AlarmNode node = GetAlarmNode(terminal.idn);
            node.alarmTriggered = true;
            node.countdownTimer = 30f;
            RefreshAlarmGraph();
        }
    }
    public void SetLevelAlarmActive() {
        if (gameData.levelState.anyAlarmTerminalActivated()) {

        } else {
            alarmSoundTimer = alarmSoundInterval;
        }
        ClearPoint[] allClearPoints = FindObjectsOfType<ClearPoint>();
        foreach (ClearPoint point in allClearPoints) {
            point.cleared = false;
        }
        if (allClearPoints.Length > 0) {
            foreach (SphereRobotAI ai in FindObjectsOfType<SphereRobotAI>()) {
                ai.OnAlarmActivate(allClearPoints);
            }
        }
        OnSuspicionChange?.Invoke();
    }
    public bool isAlarmRadioInProgress(GameObject exclude) {
        foreach (HQReport report in reports.Values) {
            if (report.desiredAlarmState == HQReport.AlarmChange.raiseAlarm && report.reporter != exclude) {
                return true;
            }
        }
        return false;
    }
    public AlarmRadio levelRadioTerminal() => alarmComponents.Values
                    .Where(node => node != null & node is AlarmRadio)
                    .Select(component => (AlarmRadio)component)
                    .FirstOrDefault();

    public void RemoveAlarmNode(string idn) {
        AlarmComponent component = GetAlarmComponent(idn);
        alarmComponents.Remove(idn);
        Destroy(component.gameObject);
        Debug.Log($"removing alarm node {idn}");
    }

    public void DeactivateAlarm() {
        if (applicationIsQuitting || isLoadingLevel)
            return;
        gameData.levelState.delta.strikeTeamResponseTimer = 0f;
        OnSuspicionChange?.Invoke();

        // reset strike team 

        PrefabPool pool = PoolManager.I?.GetPool("prefabs/NPC");
        // if (pool != null)
        //     gameData.levelState.delta.strikeTeamMaxSize = Math.Min(3, pool.objectsInPool.Count);
        gameData.levelState.delta.strikeTeamCount = 0;
    }
    public void OpenReportTicket(GameObject reporter, HQReport report) {
        if (levelRadioTerminal() != null) {
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
        if (levelRadioTerminal() != null) {
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

    public void SetLocationOfDisturbance(Vector3 location) {
        locationOfLastDisturbance = location;
    }
    void CloseReport(KeyValuePair<GameObject, HQReport> kvp) {
        SetLocationOfDisturbance(kvp.Value.locationOfLastDisturbance);
        if (kvp.Value.desiredAlarmState == HQReport.AlarmChange.raiseAlarm) {
            ActivateHQRadioNode();
            if (gameData.levelState.delta.guardPhase == GuardPhase.normal) {
                DisplayHQResponse("HQ: Understood. Dispatching strike team.");
            } else {
                DisplayHQResponse("HQ: Understood. Remain vigilant.");
            }
        } else if (kvp.Value.desiredAlarmState == HQReport.AlarmChange.cancelAlarm) {
            DeactivateAlarm();
            DisplayHQResponse("HQ: Understood. Disabling alarm.");
        } else if (kvp.Value.desiredAlarmState == HQReport.AlarmChange.noChange) {
            DeactivateAlarm();
            DisplayHQResponse("HQ: Understood. Remain vigilant.");
        }
        if (kvp.Value.suspicionRecord != null) {
            GameManager.I.AddSuspicionRecord(kvp.Value.suspicionRecord);
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
        ActivateHQRadioNode();
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
        if (gameData.levelState.anyAlarmTerminalActivated()) {
            if (strikeTeamSpawnPoint != null && levelRadioTerminal() != null) { // TODO: check level data 
                UpdateStrikeTeamSpawn();
            }
            alarmSoundTimer += Time.deltaTime;
            if (alarmSoundTimer > alarmSoundInterval) {
                alarmSoundTimer -= alarmSoundInterval;
                audioSource.PlayOneShot(gameData.levelState.template.alarmAudioClip);
            }
        }
        UpdateRegularGuardRespawn();
    }
    void UpdateRegularGuardRespawn() {
        if (gameData.levelState.delta.npcCount < gameData.levelState.template.minNPC) {
            gameData.levelState.delta.npcSpawnTimer += Time.deltaTime;
            if (gameData.levelState.delta.npcSpawnTimer > gameData.levelState.template.npcSpawnInterval) {
                gameData.levelState.delta.npcSpawnTimer -= gameData.levelState.template.npcSpawnInterval;
                SpawnGuard();
            }
        }
    }
    void UpdateStrikeTeamSpawn() {
        // Debug.Log($"{strikeTeamCount} < {gameData.levelState.delta.strikeTeamMaxSize} {strikeTeamResponseTimer} < {gameData.levelState.template.strikeTeamResponseTime}");
        if (gameData.levelState.delta.strikeTeamCount < gameData.levelState.template.strikeTeamMaxSize) {
            if (gameData.levelState.delta.strikeTeamResponseTimer < gameData.levelState.template.strikeTeamResponseTime) {
                gameData.levelState.delta.strikeTeamResponseTimer += Time.deltaTime;
            } else {
                gameData.levelState.delta.strikeTeamSpawnTimer += Time.deltaTime;
                if (gameData.levelState.delta.strikeTeamSpawnTimer > gameData.levelState.template.strikeTeamSpawnInterval) {
                    gameData.levelState.delta.strikeTeamSpawnTimer -= gameData.levelState.template.strikeTeamSpawnInterval;
                    SpawnStrikeTeamMember();
                }
            }
        }
    }

    void SpawnStrikeTeamMember() {
        // Debug.Log($"****** spawn strike member: {gameData.levelState.delta.npcsSpawned} >= {gameData.levelState.template.maxNPC}?");
        if (gameData.levelState.delta.npcCount >= gameData.levelState.template.maxNPC) return;
        GameObject npc = strikeTeamSpawnPoint.SpawnNPC(gameData.levelState.template.strikeTeamTemplate);
        SphereRobotAI ai = npc.GetComponentInChildren<SphereRobotAI>();
        CharacterController controller = npc.GetComponent<CharacterController>();
        controller.OnCharacterDead += HandleNPCDead;

        if (gameData.levelState.delta.strikeTeamCount == 0) {
            ai.ChangeState(new SearchDirectionState(ai, locationOfLastDisturbance, controller, doIntro: false, speedCoefficient: 1.5f));
            lastStrikeTeamMember = npc;
        } else if (gameData.levelState.delta.strikeTeamCount == 1) {
            ai.ChangeState(new FollowTheLeaderState(ai, lastStrikeTeamMember, controller, headBehavior: AI.TaskFollowTarget.HeadBehavior.right));
            lastStrikeTeamMember = npc;
        } else if (gameData.levelState.delta.strikeTeamCount == 2) {
            ai.ChangeState(new FollowTheLeaderState(ai, lastStrikeTeamMember, controller, headBehavior: AI.TaskFollowTarget.HeadBehavior.left));
        }
        gameData.levelState.delta.strikeTeamCount += 1;
        gameData.levelState.delta.npcCount += 1;
        OnNPCSpawn?.Invoke(npc);
    }

    void SpawnGuard() {
        if (gameData.levelState.delta.npcCount >= gameData.levelState.template.maxNPC) return;
        NPCSpawnPoint spawnPoint = Toolbox.RandomFromList(GameObject.FindObjectsOfType<NPCSpawnPoint>().Where(spawn => !spawn.isStrikeTeamSpawn).ToList());

        GameObject npc = strikeTeamSpawnPoint.SpawnNPC(spawnPoint.myTemplate);
        SphereRobotAI ai = npc.GetComponentInChildren<SphereRobotAI>();
        CharacterController controller = npc.GetComponent<CharacterController>();
        controller.OnCharacterDead += HandleNPCDead;

        gameData.levelState.delta.npcCount += 1;
        OnNPCSpawn?.Invoke(npc);
    }

    public void DispatchGuard(Vector3 position) {
        SphereRobotAI[] ais = GameObject.FindObjectsOfType<SphereRobotAI>();
        if (ais.Count() == 0) return;
        SphereRobotAI closestAI = ais.OrderBy(ai => (ai.transform.position - position).sqrMagnitude).First();
        CharacterController controller = closestAI.GetComponent<CharacterController>();
        closestAI.ChangeState(new SearchDirectionState(closestAI, position, controller, doIntro: false, speedCoefficient: 1.5f));
    }
}