using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using cakeslice;
using Easings;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
public partial class GameManager : Singleton<GameManager> {
    static readonly Dictionary<SkyBoxType, string> skyboxSceneNames = new Dictionary<SkyBoxType, string>{
        {SkyBoxType.city, "cityskybox"}
    };

    // these things should belong to level delta
    public static Dictionary<string, PoweredComponent> poweredComponents;
    public static Dictionary<string, CyberComponent> cyberComponents;
    public static Dictionary<string, AlarmComponent> alarmComponents;
    NPCSpawnPoint strikeTeamSpawnPoint;
    public float timePlayed;
    List<AsyncOperation> scenesLoading;
    public bool isLoadingLevel;
    void Awake() {
        InputController.InitializeInstance();
    }
    public void LoadVRMission(VRMissionTemplate template) {
        Debug.Log("GameMananger: load VR mission");
        LevelTemplate levelTemplate = LevelTemplate.LoadResource("test");
        // we are modifying an instance here, not the asset on disk.
        // we should perhaps do more to modify the level template based on the vr mission template.
        // this means that the gamedata is not serializable!
        // instead, NPC templates should be top level fields of levelState, and thus serializable.
        // but, we edit it from the VR mission editor.
        // so, there should be a serializable NPC template (from editor) as well as a scriptable object NPC template?
        levelTemplate.strikeTeamTemplate = template.npc2State;
        levelTemplate.sensitivityLevel = template.sensitivityLevel;
        levelTemplate.maxNPC = 3;

        LevelState levelState = LevelState.Instantiate(levelTemplate, LevelPlan.Default(new List<Items.BaseItem>()));
        // instantiate gamedata
        gameData = GameData.TestInitialData() with {
            playerState = PlayerState.Instantiate(template.playerState),
            levelState = levelState
        };

        // instantiate mission state from template
        VRMissionState state = VRMissionState.Instantiate(template);

        LoadScene(template.sceneName, () => StartVRMission(state));
    }
    public void LoadMission(LevelTemplate template, LevelPlan plan) {
        Debug.Log("GameMananger: load mission");
        gameData.levelState = LevelState.Instantiate(template, plan);
        gameData.playerState.ResetTemporaryState();
        LoadScene(template.sceneName, () => StartMission(gameData.levelState));
    }

    public void StartVRMission(VRMissionState state) {
        Debug.Log("GameMananger: start VR mission");
        if (!SceneManager.GetSceneByName("UI").isLoaded) {
            LoadScene("UI", () => {
                uiController = GameObject.FindObjectOfType<UIController>();
                uiController.Initialize();
                uiController.InitializeObjectivesController(gameData);
            }, unloadAll: false);
        }
        InitializeLevel(LevelPlan.Default(new List<Items.BaseItem>()));
        LoadSkyboxForScene(state.template.sceneName);

        TransitionToPhase(GamePhase.vrMission);
        GameObject controller = GameObject.Instantiate(Resources.Load("prefabs/VRMissionController")) as GameObject;
        VRMissionController missionController = controller.GetComponent<VRMissionController>();
        missionController.StartVRMission(state);
    }
    void LoadSkyboxForScene(String sceneName) {
        SceneData sceneData = SceneData.loadSceneData(sceneName);
        LoadSkyBox(sceneData);
    }
    public void StartMission(LevelState state, bool spawnNpcs = true, bool doCutscene = true) {
        Debug.Log($"GameMananger: start mission {state.template.levelName}");
        if (!SceneManager.GetSceneByName("UI").isLoaded) {
            LoadScene("UI", () => {
                uiController = GameObject.FindObjectOfType<UIController>();
                uiController.Initialize();
                uiController.InitializeObjectivesController(gameData);
            }, unloadAll: false);
        }
        InitializeLevel(state.plan);
        LoadSkyboxForScene(state.template.sceneName);

        playerCharacterController.OnCharacterDead += HandlePlayerDead;

        // spawn NPC
        if (spawnNpcs) {
            while (state.delta.npcsSpawned < state.template.maxInitialNPC) {
                foreach (NPCSpawnPoint spawnPoint in GameObject.FindObjectsOfType<NPCSpawnPoint>().Where(spawn => !spawn.isStrikeTeamSpawn).OrderBy(a => Guid.NewGuid()).ToList()) {
                    if (state.delta.npcsSpawned >= state.template.maxInitialNPC) continue;
                    GameObject npc = spawnPoint.SpawnTemplated();
                    CharacterController controller = npc.GetComponentInChildren<CharacterController>();
                    controller.OnCharacterDead += HandleNPCDead;
                    state.delta.npcsSpawned += 1;
                }
            }
            foreach (WorkerSpawnPoint spawnPoint in GameObject.FindObjectsOfType<WorkerSpawnPoint>()) {
                GameObject npc = spawnPoint.SpawnTemplated();
            }

            foreach (RobotSpawnPoint spawnPoint in GameObject.FindObjectsOfType<RobotSpawnPoint>().Where(spawn => !spawn.isStrikeTeamSpawn).ToList()) {
                spawnPoint.SpawnNPC(useSpawnEffect: false);
            }

            foreach (NPCSpawnZone zone in GameObject.FindObjectsOfType<NPCSpawnZone>()) {
                zone.SpawnNPCs();
            }
        }


        MusicController.I.LoadTrack(state.template.musicTrack);

        TransitionToPhase(GamePhase.levelPlay);

        lastObjectivesStatusHashCode = Toolbox.ListHashCode<ObjectiveStatus>(state.template.objectives
            .Where(objective => !objective.isOptional)
            .Select(objective => objective.Status(gameData)).ToList());

        if (doCutscene)
            StartCutsceneCoroutine(StartMissionCutscene());
    }
    public void StartWorld(string sceneName) {
        if (!SceneManager.GetSceneByName("UI").isLoaded) {
            LoadScene("UI", () => {
                uiController = GameObject.FindObjectOfType<UIController>();
                uiController.Initialize();
                uiController.HideUI();
                uiController.ShowInteractiveHighlight();
            }, unloadAll: false);
        }
        // TODO: this should be inside the loading coroutine.
        foreach (NPCSpawnZone zone in GameObject.FindObjectsOfType<NPCSpawnZone>()) {
            zone.SpawnNPCs();
        }
        InitializePlayerAndController(LevelPlan.Default(new List<Items.BaseItem>()));
        LoadSkyboxForScene(sceneName);
        // MusicController.I.LoadTrack(MusicTrack.antiAnecdote);

        TransitionToPhase(GamePhase.world);
    }
    void HandlePlayerDead(CharacterController npc) {
        gameData.levelState.delta.phase = LevelDelta.MissionPhase.playerDead;
        // FinishMission();
        StartCoroutine(WaitAndShowMissionFinish());
    }
    void HandleNPCDead(CharacterController npc) {
        gameData.levelState.delta.npcsSpawned -= 1;
        npc.OnCharacterDead -= HandleNPCDead;
    }

    IEnumerator WaitAndShowMissionFinish() {
        yield return new WaitForSeconds(2f);
        FinishMission();
    }

    public void FinishMission() {
        playerCharacterController.OnCharacterDead -= HandlePlayerDead;
        switch (gameData.levelState.delta.phase) {
            case LevelDelta.MissionPhase.extractionSuccess:
                FinishMissionSuccess();
                break;
            case LevelDelta.MissionPhase.extractionFail:
            case LevelDelta.MissionPhase.playerDead:
                FinishMissionFail();
                break;
        }
    }

    public void FinishMissionSuccess() {
        Debug.Log("mission success");
        MusicController.I.Stop();
        gameData.completedLevels.Add(gameData.levelState.template.levelName);
        gameData.playerState.credits += gameData.levelState.template.creditReward;
        gameData.playerState.payDatas.AddRange(gameData.levelState.delta.levelAcquiredPaydata);
        gameData.playerState.loots.AddRange(gameData.levelState.delta.levelAcquiredLoot);
        LoadAfterActionReport();
    }
    public void FinishMissionFail() {
        Debug.Log("mission fail");
        MusicController.I.Stop();
        GameManager.I.ShowMenu(MenuType.missionFail, () => {
            MissionFailMenuController menuController = GameObject.FindObjectOfType<MissionFailMenuController>();
            menuController.Initialize(gameData);
        });
    }

    public void ReturnToTitleScreen() {
        MusicController.I.Stop();
        LoadScene("title", () => {
            Debug.Log("start title screen");
            activeMenuType = MenuType.none;
        });
    }

    public void LoadAfterActionReport() {
        MusicController.I.Stop();
        TransitionToPhase(GamePhase.afteraction);
        LoadScene("AfterAction", () => {
            Debug.Log("start afteraction");
            // activeMenuType = MenuType.none;
            AfterActionReportHandler handler = GameObject.FindObjectOfType<AfterActionReportHandler>();
            handler.Initialize(gameData);
        });
    }

    public void LoadScene(string targetScene, Action callback, bool unloadAll = true) {
        isLoadingLevel = true;

        List<string> scenesToUnload = new List<string>();
        List<string> scenesToLoad = new List<string> { targetScene };

        if (unloadAll) {
            int sceneCount = SceneManager.sceneCount;
            for (int i = 0; i < sceneCount; i++) {
                string activeSceneName = SceneManager.GetSceneAt(i).name;
                scenesToUnload.Add(activeSceneName);
            }

            Debug.Log("show loading screen");
            SceneManager.LoadScene("LoadingScreen", LoadSceneMode.Additive);
        }

        StartCoroutine(GetSceneLoadProgress(targetScene, scenesToLoad, scenesToUnload, () => {
            foreach (LevelBootstrapper bootstrapper in GameObject.FindObjectsOfType<LevelBootstrapper>()) {
                DestroyImmediate(bootstrapper.gameObject);
            }
            if (callback != null)
                callback();

            if (unloadAll && SceneManager.GetSceneByName("LoadingScreen").isLoaded) {
                Debug.Log("remove loading screen");
                SceneManager.UnloadSceneAsync("LoadingScreen");
            }
            isLoadingLevel = false;
        }));
    }
    public IEnumerator GetSceneLoadProgress(string targetScene, List<string> scenesToLoad, List<string> scenesToUnload, Action callback) {
        scenesLoading = new List<AsyncOperation>();

        // if target scene is in scenes to unload, then that means it must be loaded right now, so we want to reload it.
        LoadSceneMode targetSceneLoadMode = scenesToUnload.Contains(targetScene) ? LoadSceneMode.Single : LoadSceneMode.Additive;
        foreach (string sceneToLoad in scenesToLoad) {
            if (sceneToLoad == targetScene) {
                Debug.Log($"loading scene async {sceneToLoad} {targetSceneLoadMode}");
                scenesLoading.Add(SceneManager.LoadSceneAsync(sceneToLoad, targetSceneLoadMode));
            } else {
                Debug.Log($"loading scene async {sceneToLoad} {LoadSceneMode.Additive}");
                scenesLoading.Add(SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Additive));
            }
        }
        // don't unload the scene we just loaded!
        if (scenesToUnload.Contains(targetScene)) {
            scenesToUnload.Remove(targetScene);
        }
        for (int i = 0; i < scenesLoading.Count; i++) {
            while (!scenesLoading[i].isDone) {
                yield return null;
            }
        }

        // TODO: better system here
        if (targetScene != "UI" && targetScene != "DialogueMenu" && targetScene != "VRMissionFinish" && targetScene != "EscapeMenu" && targetScene != "cityskybox")
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(targetScene));

        foreach (string sceneToUnload in scenesToUnload) {
            Debug.Log($"try unload scene async: {sceneToUnload} {SceneManager.GetSceneByName(sceneToUnload).isLoaded}");
            if (SceneManager.GetSceneByName(sceneToUnload).isLoaded)
                scenesLoading.Add(SceneManager.UnloadSceneAsync(sceneToUnload));
        }
        for (int i = 0; i < scenesLoading.Count; i++) {
            while (!scenesLoading[i].isDone) {
                yield return null;
            }
        }
        yield return new WaitForEndOfFrame();
        callback();
        yield return new WaitForEndOfFrame();
    }

    public void SetFocus(GameObject focus) {
        Debug.Log($"setting focus: {focus}");
        this.playerObject = focus;
        this.playerLightLevelProbe = focus.GetComponentInChildren<LightLevelProbe>();
        this.playerCharacterController = focus.GetComponentInChildren<CharacterController>();
        this.playerCollider = focus.GetComponentInChildren<Collider>();
        this.clearSighter2 = GameObject.FindObjectOfType<NeoClearsighter>();

        if (playerOutlineHandler != null) {
            playerOutlineHandler.UnBind();
        }
        playerOutlineHandler = focus.GetComponentInChildren<PlayerOutlineHandler>();
        playerOutlineHandler?.Bind();

        ClearSighter clearSighter = GameObject.FindObjectOfType<ClearSighter>();
        // if (clearSighter == null) {
        //     // instantiate clearsighter
        // }
        if (clearSighter != null && focus != null) {
            // clearSighter.Initialize(focus.transform);
            Destroy(clearSighter);
        }
        if (clearSighter2 != null) {
            clearSighter2.Initialize(focus.transform, characterCamera);
            // Destroy(clearSighter2);
        }

        GunHandler handler = focus.GetComponentInChildren<GunHandler>();
        handler.SetGunAppearanceSuspicion();

        OnFocusChanged?.Invoke(focus);
        OnEyeVisibilityChange?.Invoke(gameData.playerState);
    }
    public bool IsObjectVisible(GameObject obj) {
        return clearSighter2?.IsObjectVisible(obj) ?? true;
    }
    void ClearSceneData() {
        // this stuff should all belong to level delta
        poweredComponents = new Dictionary<string, PoweredComponent>();
        cyberComponents = new Dictionary<string, CyberComponent>();
        alarmComponents = new Dictionary<string, AlarmComponent>();
        lastStrikeTeamMember = null;

        // TODO: this stuff should belong to level state
        reports = new Dictionary<GameObject, HQReport>();
        suspicionRecords = new Dictionary<string, SuspicionRecord>();
        lastObjectivesStatusHashCode = -1;
    }
    private void InitializePlayerAndController(LevelPlan plan) {
        ClearSceneData();
        // inputController = GameObject.FindObjectOfType<InputController>();
        characterCamera = GameObject.FindObjectOfType<CharacterCamera>();
        InputController.I.OrbitCamera = characterCamera;
        characterCamera.outlineEffect.Initialize();
        foreach (Outline outline in GameObject.FindObjectsOfType<Outline>()) {
            outline.OnEnable();
            outline.OnDisable();
        }
        // spawn player object  
        GameObject playerObj = SpawnPlayer(gameData.playerState, plan);
        SetFocus(playerObj);

        // connect player object to input controller
        InputController.I.SetInputReceivers(playerObj);

    }
    public void LoadSkyBox(SceneData sceneData) {
        SkyBoxType skyBoxType = sceneData?.skyBoxType ?? SkyBoxType.none;
        if (skyBoxType == SkyBoxType.none) return;
        string skyboxSceneName = skyboxSceneNames[skyBoxType];
        LoadScene(skyboxSceneName, () => {
            List<Camera> skycams = new List<Camera>();
            foreach (Skycam skycam in FindObjectsOfType<Skycam>()) {
                skycams.Add(skycam.myCamera);
                skycam.Initialize(characterCamera.Camera, sceneData.skyboxOffset);
            }
            characterCamera.skyBoxCameras = skycams.ToArray();
        }, unloadAll: false);
    }
    private void InitializeLevel(LevelPlan plan) {
        InitializePlayerAndController(plan);

        // apply level initializer
        LevelInitializer initializer = GameObject.FindObjectOfType<LevelInitializer>();
        initializer?.ApplyState();

        // connect up power grids
        Debug.Log("connecting power grid...");
        foreach (PoweredComponent component in GameObject.FindObjectsOfType<PoweredComponent>()) {
            poweredComponents[component.idn] = component;
        }

        // connect up cyber grids
        Debug.Log("connecting cyber grid...");
        foreach (CyberComponent component in GameObject.FindObjectsOfType<CyberComponent>()) {
            cyberComponents[component.idn] = component;
        }

        // connect up alarm grids
        Debug.Log("connecting alarm grid...");
        foreach (AlarmComponent component in GameObject.FindObjectsOfType<AlarmComponent>()) {
            alarmComponents[component.idn] = component;
        }

        RefreshPowerGraph();
        RefreshCyberGraph();
        RefreshAlarmGraph();

        alarmSoundInterval = 2f;
        alarmSound = gameData.levelState.template.alarmAudioClip;
        strikeTeamSpawnPoint = GameObject.FindObjectsOfType<NPCSpawnPoint>().FirstOrDefault(spawn => spawn.isStrikeTeamSpawn);

        OnSuspicionChange?.Invoke();
    }

    // TODO: this belongs to level logic, but it's fine to put it here for now
    public void SetNodeEnabled<T, U>(T graphNodeComponent, bool state) where T : GraphNodeComponent<T, U> where U : Node {
        if (isLoadingLevel) return;

        string idn = graphNodeComponent.idn;

        Node node = graphNodeComponent switch {
            PoweredComponent => GetPowerNode(idn),
            CyberComponent => GetCyberNode(idn),
            AlarmComponent => GetAlarmNode(idn),
            GraphNodeComponent<PoweredComponent, PowerNode> => GetPowerNode(idn),
            GraphNodeComponent<CyberComponent, CyberNode> => GetCyberNode(idn),
            GraphNodeComponent<AlarmComponent, AlarmNode> => GetAlarmNode(idn),
            _ => null
        };

        if (node != null) {
            // node.enabled = state;
            node.setEnabled(state);
            graphNodeComponent.nodeEnabled = state;
            switch (graphNodeComponent) {
                case PoweredComponent:
                    RefreshPowerGraph();
                    break;
                case CyberComponent:
                    RefreshCyberGraph();
                    break;
                case AlarmComponent:
                    AlarmNode alarmNode = (AlarmNode)node;
                    alarmNode.alarmTriggered = false;
                    alarmNode.countdownTimer = 0f;
                    // Debug.Log($"disabling alarm node {graphNodeComponent} {node}");
                    RefreshAlarmGraph();
                    break;
            };
        } else {
            // Debug.Log($"called set node enabled with null node {graphNodeComponent} {idn}");
        }
    }

    public void SetAlarmOverride(AlarmComponent component) {
        string idn = component.idn;
        AlarmNode node = GetAlarmNode(idn);
        node.overrideState = AlarmNode.AlarmOverrideState.disabled;
    }

    // TODO: these methods could belong to the graphs?
    public CyberNode GetCyberNode(string idn) {
        return gameData?.levelState?.delta.cyberGraph?.nodes.ContainsKey(idn) ?? false ? gameData.levelState.delta.cyberGraph.nodes[idn] : null;
    }
    public PowerNode GetPowerNode(string idn) {
        return gameData?.levelState?.delta.powerGraph?.nodes.ContainsKey(idn) ?? false ? gameData.levelState.delta.powerGraph.nodes[idn] : null;
    }
    public AlarmNode GetAlarmNode(string idn) {
        return gameData?.levelState?.delta.alarmGraph?.nodes.ContainsKey(idn) ?? false ? gameData.levelState.delta.alarmGraph.nodes[idn] : null;
    }
    public AlarmComponent GetAlarmComponent(string idn) {
        if (alarmComponents == null)
            return null;
        if (alarmComponents.ContainsKey(idn)) {
            return alarmComponents[idn];
        } else return null;
    }
    public PoweredComponent GetPowerComponent(string idn) {
        if (poweredComponents == null)
            return null;
        if (poweredComponents.ContainsKey(idn)) {
            return poweredComponents[idn];
        } else return null;
    }
    public CyberComponent GetCyberComponent(string idn) {
        if (cyberComponents == null)
            return null;
        if (cyberComponents.ContainsKey(idn)) {
            return cyberComponents[idn];
        } else return null;
    }
    public void SetPowerNodeState(PoweredComponent poweredComponent, bool state) {
        if (applicationIsQuitting) return;

        string idn = poweredComponent.idn;
        if (gameData.levelState != null && gameData.levelState.delta.powerGraph != null && gameData.levelState.delta.powerGraph.nodes.ContainsKey(idn)) {
            // Debug.Log($"setting power node power {idn} {state}");
            gameData.levelState.delta.powerGraph.nodes[idn].powered = state;
            RefreshPowerGraph();
        }
    }
    public void SetCyberNodeState(CyberComponent cyberComponent, bool state) {
        if (applicationIsQuitting) return;

        string idn = cyberComponent.idn;
        if (gameData.levelState != null && gameData.levelState.delta.cyberGraph != null && gameData.levelState.delta.cyberGraph.nodes.ContainsKey(idn)) {
            SetCyberNodeState(gameData.levelState.delta.cyberGraph.nodes[idn], state);
        }
    }
    public void SetCyberNodeState(CyberNode node, bool state) {
        if (applicationIsQuitting) return;

        node.compromised = state;
        RefreshCyberGraph();
    }
    public void SetAlarmNodeState(AlarmComponent alarmComponent, bool state) {
        if (applicationIsQuitting) return;

        string idn = alarmComponent.idn;
        if (gameData.levelState != null && gameData.levelState.delta.alarmGraph != null && gameData.levelState.delta.alarmGraph.nodes.ContainsKey(idn)) {
            SetAlarmNodeState(gameData.levelState.delta.alarmGraph.nodes[idn], state);
        }
    }

    public void SetAlarmNodeState(AlarmNode node, bool state) {
        if (applicationIsQuitting) return;
        if (node == null) return;
        if (node.getEnabled()) {
            node.alarmTriggered = state;
            if (state) {
                node.countdownTimer = 30f;
            } else {
                node.countdownTimer = 0f;
            }
            RefreshAlarmGraph();
        }
    }
    public bool IsCyberNodeVulnerable(CyberNode node) {

        if (node.compromised)
            return false;
        if (gameData.levelState != null && gameData.levelState.delta.cyberGraph != null && gameData.levelState.delta.cyberGraph.nodes.ContainsKey(node.idn)) {
            foreach (CyberNode neighbor in gameData.levelState.delta.cyberGraph.Neighbors(node)) {
                if (neighbor.compromised) return true;
            }
            return false;
        } else return false;
    }

    public void RefreshCyberGraph() {
        if (applicationIsQuitting) return;

        gameData.levelState.delta.cyberGraph.Refresh();

        TransferCyberState();

        // propagate changes to UI
        OnCyberGraphChange?.Invoke(gameData.levelState.delta.cyberGraph);
    }
    public void RefreshAlarmGraph() {
        if (applicationIsQuitting) return;
        // determine if any active alarm object reaches a terminal
        gameData.levelState.delta.alarmGraph.Refresh();

        TransferAlarmState();

        // propagate changes to UI
        OnAlarmGraphChange?.Invoke(gameData.levelState.delta.alarmGraph);

        bool alarmActive = gameData.levelState.anyAlarmActive();
        if (alarmActive) {
            SetLevelAlarmActive();
        } else {
            DeactivateAlarm();
        }
    }
    public void RefreshPowerGraph() {
        if (applicationIsQuitting) return;

        // power distribution algorithm
        gameData.levelState.delta.powerGraph.Refresh();

        // propagate the changes to local state
        TransferPowerState();

        // propagate changes to UI
        OnPowerGraphChange?.Invoke(gameData.levelState.delta.powerGraph);
    }
    void TransferPowerState() {
        if (poweredComponents == null)
            return;
        foreach (KeyValuePair<string, PowerNode> kvp in gameData.levelState.delta.powerGraph.nodes) {
            if (poweredComponents.ContainsKey(kvp.Key)) {
                poweredComponents[kvp.Key].power = kvp.Value.powered;
                poweredComponents[kvp.Key].nodeEnabled = kvp.Value.getEnabled();
                // Debug.Log($"transfer power to {kvp.Key}: {kvp.Value.powered}");
            }
        }
    }
    void TransferCyberState() {
        if (cyberComponents == null)
            return;
        foreach (KeyValuePair<string, CyberNode> kvp in gameData.levelState.delta.cyberGraph.nodes) {
            if (cyberComponents.ContainsKey(kvp.Key)) {
                cyberComponents[kvp.Key].compromised = kvp.Value.compromised;
                cyberComponents[kvp.Key].nodeEnabled = kvp.Value.getEnabled();
                // Debug.Log($"transfer cyber to {kvp.Key}: {kvp.Value.enabled}");
            }
        }
    }
    void TransferAlarmState() {
        if (alarmComponents == null)
            return;
        foreach (KeyValuePair<string, AlarmNode> kvp in gameData.levelState.delta.alarmGraph.nodes) {
            if (alarmComponents.ContainsKey(kvp.Key)) {
                AlarmComponent component = alarmComponents[kvp.Key];
                component.alarmTriggered = kvp.Value.alarmTriggered;
                component.countdownTimer = kvp.Value.countdownTimer;
                component.nodeEnabled = kvp.Value.getEnabled();
            }
        }
    }

    GameObject SpawnPlayer(PlayerState state, LevelPlan plan) {
        if (plan.insertionPointIdn != "") { // default
            foreach (PlayerSpawnPoint point in GameObject.FindObjectsOfType<PlayerSpawnPoint>()) {
                if (point.data.idn == plan.insertionPointIdn)
                    return point.SpawnPlayer(state, plan);
            }
        }
        PlayerSpawnPoint spawnPoint = GameObject.FindObjectOfType<PlayerSpawnPoint>();
        return spawnPoint.SpawnPlayer(state, plan);
    }

    public DialogueInput GetDialogueInput(SphereRobotAI ai) => new DialogueInput() {
        NPCAI = ai,
        npcObject = ai.gameObject,

        playerObject = playerObject,
        playerState = gameData.playerState,
        levelState = gameData.levelState,
        suspicionRecords = suspicionRecords,
        playerSuspiciousness = GetTotalSuspicion(),
        alarmActive = gameData.levelState.delta.alarmGraph.anyAlarmActive(),
        playerInDisguise = gameData.levelState.delta.disguise,
        playerSpeechSkill = gameData.playerState.speechSkillLevel,
        playerHasID = gameData.levelState.PlayerHasID()
    };
}