using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KinematicCharacterController;
using UnityEngine;
public partial class GameManager : Singleton<GameManager> {

    public enum HQPhase { normal, dispatchStrikeTeam, strikeTeamMission }
    public float alarmSoundTimer;
    public float alarmSoundInterval;

    // can't be part of level state: not serializable?
    public Vector3 locationOfLastDisturbance;
    public GameObject lastStrikeTeamMember;
    public Dictionary<GameObject, HQReport> reports;
    float clearCaptionTimer;
    public Action<GameObject> OnNPCSpawn;

    List<CharacterController> strikeTeamMembers;

    public void ActivateHQRadioNode() {
        AlarmRadio terminal = levelRadioTerminal();
        if (terminal != null) {
            SetAlarmNodeTriggered(terminal, true);
        }
    }
    void ChangeHQPhase(HQPhase newPhase) {
        // Debug.Log($"**** CHANGING HQ PHASE {newPhase}");
        gameData.levelState.delta.hqPhase = newPhase;
        if (newPhase == HQPhase.normal || newPhase == HQPhase.dispatchStrikeTeam) {
            gameData.levelState.delta.strikeTeamMissionTimer = 0f;
        }
    }
    public void ActivateLevelAlarm() {
        gameData.levelState.delta.strikeTeamMissionTimer = 0f;
        if (!gameData.levelState.delta.alarmTerminalActive) {
            if (gameData.levelState.delta.hqPhase == HQPhase.normal)
                ChangeHQPhase(HQPhase.dispatchStrikeTeam);

            gameData.levelState.delta.alarmTerminalActive = true;
            alarmSoundTimer = alarmSoundInterval;

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
    }
    public void DeactivateAlarm() {
        if (applicationIsQuitting || isLoadingLevel || !gameData.levelState.delta.alarmTerminalActive)
            return;

        gameData.levelState.delta.alarmTerminalActive = false;
        gameData.levelState.delta.strikeTeamResponseTimer = 0f;
        OnSuspicionChange?.Invoke();
        PrefabPool pool = PoolManager.I?.GetPool("prefabs/NPC");

        // TODO: correct?
        gameData.levelState.delta.strikeTeamCount = 0;
        GameManager.I.gameData.levelState.delta.strikeTeamBehavior = GameManager.I.gameData.levelState.template.strikeTeamBehavior;
        ChangeHQPhase(HQPhase.normal);
        foreach (AlarmComponent alarmComponent in FindObjectsOfType<AlarmComponent>()) {
            if (GetAlarmNodeTriggered(alarmComponent)) {
                SetAlarmNodeTriggered(alarmComponent, false);
            }
        }
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
            GameManager.I.gameData.levelState.delta.strikeTeamBehavior = LevelTemplate.StrikeTeamResponseBehavior.investigate;
            ActivateHQRadioNode();
            if (gameData.levelState.delta.hqPhase == HQPhase.normal) {
                DisplayHQResponse("HQ: Understood. Dispatching strike team.");
            } else {
                DisplayHQResponse("HQ: Understood. Remain vigilant.");
            }
        } else if (kvp.Value.desiredAlarmState == HQReport.AlarmChange.cancelAlarm) {
            DeactivateAlarm();
            DisplayHQResponse("HQ: Understood. Disabling alarm.");
        } else if (kvp.Value.desiredAlarmState == HQReport.AlarmChange.noChange) {
            // DeactivateAlarm();
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
            SpeechTextController speechTextController = ai.speechTextController;
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
        if (gameData.levelState.delta.alarmGraph == null) return 0f;
        foreach (AlarmNode node in gameData.levelState.delta.alarmGraph.nodes.Values) {
            timer = Math.Max(timer, node.countdownTimer);
        }
        return timer;
    }
    public void UpdateGraphs() {
        // method for time-updating graph state

        float alarmTimerOrig = alarmCountdown();

        gameData?.levelState?.delta.alarmGraph?.Update();

        float alarmTimer = alarmCountdown();
        if (alarmTimer <= 0 && alarmTimerOrig > 0) {
            InitiateAlarmShutdown();
        }
    }
    public void UpdateAlarm() {
        if (gameData.levelState.anyAlarmTerminalActivated()) {
            alarmSoundTimer += Time.deltaTime;
            if (alarmSoundTimer > alarmSoundInterval) {
                alarmSoundTimer -= alarmSoundInterval;
                audioSource.PlayOneShot(gameData.levelState.template.alarmAudioClip);
            }
        }
    }

    void UpdateNPCSpawning() {
        if (strikeTeamSpawnPoint != null && gameData.levelState.delta.hqPhase == HQPhase.dispatchStrikeTeam) {
            UpdateStrikeTeamSpawn();
        }
        UpdateRegularGuardRespawn();

        if (gameData.levelState.delta.hqPhase == HQPhase.strikeTeamMission) {
            gameData.levelState.delta.strikeTeamMissionTimer += Time.deltaTime;
            if (gameData.levelState.delta.strikeTeamMissionTimer >= 30f) {
                StrikeTeamMissionComplete();
            }
        }
    }

    public void StrikeTeamMissionComplete() {
        gameData.levelState.delta.strikeTeamMissionTimer = 0f;
        for (int strikeTeamCount = 0; strikeTeamCount < strikeTeamMembers.Count; strikeTeamCount++) {
            CharacterController controller = strikeTeamMembers[strikeTeamCount];
            SphereRobotAI ai = controller.GetComponent<SphereRobotAI>();
            AI.TaskFollowTarget.HeadBehavior headBehavior = AI.TaskFollowTarget.HeadBehavior.right;
            if (strikeTeamCount == 0) {
                if (gameData.levelState.template.strikeTeamCompletion == LevelTemplate.StrikeTeamCompletionBehavior.patrol) {
                    ai.ChangeState(new SpherePatrolState(ai, ai.patrolRoute, controller));
                } else if (gameData.levelState.template.strikeTeamCompletion == LevelTemplate.StrikeTeamCompletionBehavior.leave) {
                    ai.ChangeState(new SphereExitLevelState(ai, controller));
                }
                lastStrikeTeamMember = controller.gameObject;
            } else if (strikeTeamCount > 0) {
                ai.ChangeState(new FollowTheLeaderState(ai, lastStrikeTeamMember, controller, headBehavior: headBehavior));
                lastStrikeTeamMember = controller.gameObject;
                headBehavior = headBehavior switch {
                    AI.TaskFollowTarget.HeadBehavior.right => AI.TaskFollowTarget.HeadBehavior.left,
                    AI.TaskFollowTarget.HeadBehavior.left => AI.TaskFollowTarget.HeadBehavior.right,
                    _ => AI.TaskFollowTarget.HeadBehavior.right
                };
            }
        }

        ChangeHQPhase(HQPhase.normal);
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
        } else if (gameData.levelState.delta.strikeTeamCount >= gameData.levelState.template.strikeTeamMaxSize) {
            ChangeHQPhase(HQPhase.strikeTeamMission);
        }
    }

    void SpawnStrikeTeamMember() {
        // Debug.Log($"****** spawn strike member: {gameData.levelState.delta.npcsSpawned} >= {gameData.levelState.template.maxNPC}?");
        if (gameData.levelState.delta.npcCount >= gameData.levelState.template.maxNPC) {
            if (gameData.levelState.delta.hqPhase == HQPhase.dispatchStrikeTeam) {
                ChangeHQPhase(HQPhase.strikeTeamMission);
            }
            return;
        }
        GameObject npc = strikeTeamSpawnPoint.SpawnNPC(gameData.levelState.template.strikeTeamTemplate);
        SphereRobotAI ai = npc.GetComponentInChildren<SphereRobotAI>();
        CharacterController controller = npc.GetComponent<CharacterController>();
        controller.isStrikeTeamMember = true;
        ai.isStrikeTeamMember = true;

        controller.OnCharacterDead += HandleNPCDead;

        if (gameData.levelState.delta.strikeTeamCount == 0) {
            if (gameData.levelState.delta.strikeTeamBehavior == LevelTemplate.StrikeTeamResponseBehavior.clear) {
                ai.ChangeState(new SphereClearPointsState(ai, controller, FindObjectsOfType<ClearPoint>()));
            } else {
                ai.ChangeState(new SearchDirectionState(ai, locationOfLastDisturbance, controller, doIntro: false, speedCoefficient: 1.5f));
            }
            lastStrikeTeamMember = npc;
        } else if (gameData.levelState.delta.strikeTeamCount == 1) {
            ai.ChangeState(new FollowTheLeaderState(ai, lastStrikeTeamMember, controller, headBehavior: AI.TaskFollowTarget.HeadBehavior.right));
            lastStrikeTeamMember = npc;
        } else if (gameData.levelState.delta.strikeTeamCount == 2) {
            ai.ChangeState(new FollowTheLeaderState(ai, lastStrikeTeamMember, controller, headBehavior: AI.TaskFollowTarget.HeadBehavior.left));
        }
        gameData.levelState.delta.strikeTeamCount += 1;
        gameData.levelState.delta.npcCount += 1;
        strikeTeamMembers.Add(controller);
        OnNPCSpawn?.Invoke(npc);

        GameManager.I.gameData.levelState.delta.strikeTeamBehavior = GameManager.I.gameData.levelState.template.strikeTeamBehavior;
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