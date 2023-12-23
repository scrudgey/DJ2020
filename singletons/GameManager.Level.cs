using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using cakeslice;
using Easings;
using Items;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
public partial class GameManager : Singleton<GameManager> {
    static readonly Dictionary<SkyBoxType, string> skyboxSceneNames = new Dictionary<SkyBoxType, string>{
        {SkyBoxType.city, "cityskybox"}
    };

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

        PlayerState playerState = PlayerState.Instantiate(template.playerState);
        LevelState levelState = LevelState.Instantiate(levelTemplate, LevelPlan.Default(new List<ItemTemplate>()), playerState);
        // instantiate gamedata
        gameData = GameData.TestInitialData() with {
            playerState = playerState,
            levelState = levelState
        };

        // instantiate mission state from template
        VRMissionState state = VRMissionState.Instantiate(template);

        LoadScene(template.sceneName, () => StartVRMission(state));
    }
    public void LoadMission(LevelTemplate template, LevelPlan plan) {
        Debug.Log("GameMananger: load mission");
        gameData.levelState = LevelState.Instantiate(template, plan, gameData.playerState);
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
        InitializeLevel(LevelPlan.Default(new List<ItemTemplate>()));
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
        GameManager.I.gameData.levelState.delta.strikeTeamBehavior = state.template.strikeTeamBehavior;
        playerCharacterController.OnCharacterDead += HandlePlayerDead;

        // spawn NPC
        if (spawnNpcs) {
            while (state.delta.npcCount < state.template.maxInitialNPC) {
                foreach (NPCSpawnPoint spawnPoint in GameObject.FindObjectsOfType<NPCSpawnPoint>().Where(spawn => !spawn.isStrikeTeamSpawn).OrderBy(a => Guid.NewGuid()).ToList()) {
                    if (state.delta.npcCount >= state.template.maxInitialNPC) continue;
                    GameObject npc = spawnPoint.SpawnTemplated();
                    CharacterController controller = npc.GetComponentInChildren<CharacterController>();
                    controller.OnCharacterDead += HandleNPCDead;
                    state.delta.npcCount += 1;
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

        // randomize doors
        foreach (DoorRandomizer doorRandomizer in GameObject.FindObjectsOfType<DoorRandomizer>()) {
            doorRandomizer.ApplyState(state.template);
        }

        state.delta.cyberGraph.InfillRandomData();

        // TODO: apply level plan tactic information


        // TODO: filter out anything placed by level plan tactic information
        List<ObjectiveData> dataObjectives = state.template.objectives
            .Where(objective => objective is ObjectiveData)
            .Select(objective => (ObjectiveData)objective)
            .ToList();

        state.delta.cyberGraph.ApplyObjectiveData(dataObjectives);




        MusicController.I.LoadTrack(state.template.musicTrack);

        TransitionToPhase(GamePhase.levelPlay);

        lastObjectivesStatusHashCode = Toolbox.ListHashCode<ObjectiveStatus>(state.template.objectives
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
        InitializePlayerAndController(LevelPlan.Default(new List<ItemTemplate>()));
        LoadSkyboxForScene(sceneName);
        // MusicController.I.LoadTrack(MusicTrack.antiAnecdote);

        TransitionToPhase(GamePhase.world);
    }
    void HandlePlayerDead(CharacterController npc) {
        gameData.levelState.delta.phase = LevelDelta.MissionPhase.playerDead;
        GameObject.Instantiate(Resources.Load("prefabs/listener"), npc.transform.position, Quaternion.identity);
        StartCoroutine(WaitAndShowMissionFinish());
    }
    void HandleNPCDead(CharacterController npc) {
        RemoveNPC(npc);
    }

    public void RemoveNPC(CharacterController npc) {
        gameData.levelState.delta.npcCount -= 1;
        npc.OnCharacterDead -= HandleNPCDead;
        if (npc.isStrikeTeamMember) {
            RemoveStrikeTeamMember(npc);
        }
    }
    public void RemoveStrikeTeamMember(CharacterController npc) {
        gameData.levelState.delta.strikeTeamCount -= 1;
        strikeTeamMembers.Remove(npc);
        if (gameData.levelState.delta.strikeTeamCount == 0) {
            ChangeHQPhase(HQPhase.normal);
        }
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
        gameData.playerState.credits += gameData.levelState.delta.levelAcquiredCredits;
        gameData.playerState.payDatas.AddRange(gameData.levelState.delta.levelAcquiredPaydata.Where(data => data.type == PayData.DataType.pay));
        gameData.playerState.loots.AddRange(gameData.levelState.delta.levelAcquiredLoot.Where(loot => loot.isCollectible));
        gameData.playerState.skillpoints += 1;

        // TODO: broken
        CharacterHurtable playerHurtable = playerObject.GetComponentInChildren<CharacterHurtable>();
        gameData.playerState.health = playerHurtable.health;
        foreach (LevelTemplate unlock in gameData.levelState.template.unlockLevels) {
            if (gameData.unlockedLevels.Contains(unlock.levelName)) continue;
            gameData.unlockedLevels.Add(unlock.levelName);
        }
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
            // Debug.Log("start title screen");
            activeMenuType = MenuType.none;
        });
    }

    public void LoadAfterActionReport() {
        MusicController.I.Stop();
        TransitionToPhase(GamePhase.afteraction);
        LoadScene("AfterAction", () => {
            // Debug.Log("start afteraction");
            // activeMenuType = MenuType.none;
            // AfterActionReportHandler handler = GameObject.FindObjectOfType<AfterActionReportHandler>();
            // handler.Initialize(gameData);
            NeoAfterActionReport handler = GameObject.FindObjectOfType<NeoAfterActionReport>();
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

            // Debug.Log("show loading screen");
            SceneManager.LoadScene("LoadingScreen", LoadSceneMode.Additive);
        }

        StartCoroutine(GetSceneLoadProgress(targetScene, scenesToLoad, scenesToUnload, () => {
            foreach (LevelBootstrapper bootstrapper in GameObject.FindObjectsOfType<LevelBootstrapper>()) {
                DestroyImmediate(bootstrapper.gameObject);
            }
            if (callback != null)
                callback();

            if (unloadAll && SceneManager.GetSceneByName("LoadingScreen").isLoaded) {
                // Debug.Log("remove loading screen");
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
        if (playerGunHandler != null) {
            playerGunHandler.isPlayerCharacter = false;
        }
        this.playerObject = focus;
        this.playerLightLevelProbe = focus.GetComponentInChildren<LightLevelProbe>();
        this.playerCharacterController = focus.GetComponentInChildren<CharacterController>();
        this.playerGunHandler = focus.GetComponentInChildren<GunHandler>();
        this.playerCollider = focus.GetComponentInChildren<Collider>();
        this.playerManualHacker = focus.GetComponentInChildren<ManualHacker>();
        this.playerItemHandler = focus.GetComponentInChildren<ItemHandler>();

        ClearSighter clearSighter = GameObject.FindObjectOfType<ClearSighter>();
        this.clearSighter2 = GameObject.FindObjectOfType<NeoClearsighter>();
        this.clearSighterV3 = GameObject.FindObjectOfType<NeoClearsighterV3>();

        playerGunHandler.isPlayerCharacter = true;

        ElevatorOccluder elevatorOccluder = GameObject.FindObjectOfType<ElevatorOccluder>();

        if (playerOutlineHandler != null) {
            playerOutlineHandler.UnBind();
        }
        playerOutlineHandler = focus.GetComponentInChildren<PlayerOutlineHandler>();
        playerOutlineHandler?.Bind();

        if (clearSighter != null) {
            // clearSighter.Initialize(focus.transform);
            Destroy(clearSighter);
        }
        if (clearSighter2 != null) {
            // 
            // Debug.Log("hi");
            // clearSighter2.Initialize(focus.transform, characterCamera, playerCharacterController);
            Destroy(clearSighter2);
        }
        if (clearSighterV3 != null) {
            // Destroy(clearsighterV3);
            clearSighterV3.Initialize(focus.transform, characterCamera, playerCharacterController);
        }


        if (elevatorOccluder != null) {
            elevatorOccluder.Initialize(characterCamera);
        }

        GunHandler handler = focus.GetComponentInChildren<GunHandler>();
        handler.SetGunAppearanceSuspicion();

        OnFocusChanged?.Invoke(focus);
        OnEyeVisibilityChange?.Invoke(gameData.playerState);
    }
    public bool IsObjectVisible(GameObject obj) {
        // TODO: fix
        // if (clearSighter2 == null) return true;
        return clearSighterV3?.IsObjectVisible(obj) ?? true;
    }
    void ClearSceneData() {
        // this stuff should all belong to level delta
        lastStrikeTeamMember = null;

        // TODO: this stuff should belong to level state
        reports = new Dictionary<GameObject, HQReport>();
        strikeTeamMembers = new List<CharacterController>();
        suspicionRecords = new Dictionary<string, SuspicionRecord>();
        lastObjectivesStatusHashCode = -1;
    }
    private void InitializePlayerAndController(LevelPlan plan) {
        ClearSceneData();
        // inputController = GameObject.FindObjectOfType<InputController>();
        characterCamera = GameObject.FindObjectOfType<CharacterCamera>();
        if (characterCamera == null) {
            Debug.Break();
        }
        if (InputController.I == null) {
            Debug.Break();
        }
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
        SetInputReceivers(playerObj);
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


        // connect up power grids
        Debug.Log("connecting power grid...");
        foreach (PoweredComponent component in GameObject.FindObjectsOfType<PoweredComponent>()) {
            PowerNode node = GetPowerNode(component.idn);
            component.node = node;
            node.Bind(component.HandleNodeChange);
            component.HandleNodeChange();
            foreach (INodeBinder<PowerNode> binder in component.GetComponentsInChildren<INodeBinder<PowerNode>>()) {
                binder.Bind(node);
            }
        }

        // connect up cyber grids
        Debug.Log("connecting cyber grid...");
        foreach (CyberComponent component in FindObjectsOfType<CyberComponent>()) {
            CyberNode node = GetCyberNode(component.idn);
            component.node = node;
            node.Bind(component.HandleNodeChange);
            component.HandleNodeChange();
            foreach (INodeBinder<CyberNode> binder in component.GetComponentsInChildren<INodeBinder<CyberNode>>()) {
                binder.Bind(node);
            }
        }

        // connect up alarm grids
        Debug.Log("connecting alarm grid...");
        foreach (AlarmComponent component in FindObjectsOfType<AlarmComponent>()) {
            AlarmNode node = GetAlarmNode(component.idn);
            component.node = node;
            node.Bind(component.HandleNodeChange);
            component.HandleNodeChange();
            foreach (INodeBinder<AlarmNode> binder in component.GetComponentsInChildren<INodeBinder<AlarmNode>>()) {
                binder.Bind(node);
            }
        }

        RefreshPowerGraph();
        RefreshCyberGraph();
        RefreshAlarmGraph();

        alarmSoundInterval = 2f;
        // alarmSound = gameData.levelState.template.alarmAudioClip;
        strikeTeamSpawnPoint = GameObject.FindObjectsOfType<NPCSpawnPoint>().FirstOrDefault(spawn => spawn.isStrikeTeamSpawn);

        OnSuspicionChange?.Invoke();
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

    public DialogueInput GetDialogueInput(GameObject gameObject, DialogueCharacterInput npcCharacter) => new DialogueInput() {
        // NPCAI = ai,
        playerName = gameData.filename,
        npcObject = gameObject,
        npcCharacter = npcCharacter,

        playerObject = playerObject,
        playerState = gameData.playerState,
        levelState = gameData.levelState,
        suspicionRecords = suspicionRecords,
        playerSuspiciousness = GetTotalSuspicion(),
        alarmActive = gameData.levelState.delta.alarmGraph.anyAlarmTerminalActivated(),
        playerInDisguise = gameData.levelState.delta.disguise,
        playerSpeechSkill = gameData.playerState.PerkSpeechlevel(),
        playerHasID = gameData.levelState.PlayerHasID()
    };
}