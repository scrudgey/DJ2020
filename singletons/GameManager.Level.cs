using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Easings;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
public partial class GameManager : Singleton<GameManager> {
    static Dictionary<string, HashSet<PoweredComponent>> poweredComponents;
    static Dictionary<string, HashSet<CyberComponent>> cyberComponents;
    static Dictionary<string, AlarmComponent> alarmComponents;
    NPCSpawnPoint strikeTeamSpawnPoint;

    List<AsyncOperation> scenesLoading;

    public void StartVRMission(VRMissionData data) {
        gameData.playerData = data.playerState;
        gameData.levelData = LevelData.Load("test");
        gameData.levelData.sensitivityLevel = data.sensitivityLevel;
        LoadScene(data.sceneName);
    }

    void LoadScene(string sceneName) {
        scenesLoading = new List<AsyncOperation>();
        Debug.Log("show loading screen");
        scenesLoading.Add(SceneManager.LoadSceneAsync(sceneName));
        // scenesLoading.Add(SceneManager.UnloadSceneAsync("title"));
        StartCoroutine(GetSceneLoadProgress(sceneName));
    }
    public IEnumerator GetSceneLoadProgress(string targetScene) {
        for (int i = 0; i < scenesLoading.Count; i++) {
            while (!scenesLoading[i].isDone) {
                yield return null;
            }
        }
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(targetScene));

        Debug.Log("hide loading screen");
        TransitionToState(GameState.levelPlay);
    }

    public void SetFocus(GameObject focus) {
        this.playerObject = focus;
        this.playerLightLevelProbe = focus.GetComponentInChildren<LightLevelProbe>();

        if (playerOutlineHandler != null) {
            playerOutlineHandler.UnBind();
        }
        playerOutlineHandler = focus.GetComponentInChildren<PlayerOutlineHandler>();
        playerOutlineHandler?.Bind();

        ClearSighter clearSighter = GameObject.FindObjectOfType<ClearSighter>();
        if (clearSighter != null && focus != null)
            clearSighter.followTransform = focus.transform;

        Debug.Log($"on focus changed: {focus}");
        OnFocusChanged?.Invoke(focus);
    }
    void ClearSceneData() {
        poweredComponents = new Dictionary<string, HashSet<PoweredComponent>>();
        cyberComponents = new Dictionary<string, HashSet<CyberComponent>>();
        alarmComponents = new Dictionary<string, AlarmComponent>();
    }
    private void InitializeLevel() {
        // TODO: load and set up asynchronously behind a screen
        ClearSceneData();

        // spawn player object
        GameObject playerObj = SpawnPlayer(gameData.playerData);
        SetFocus(playerObj);

        // TODO: spawn clearsighter if necessary

        // connect player object to input controller
        InputController inputController = GameObject.FindObjectOfType<InputController>();
        inputController.SetInputReceivers(playerObj);

        // spawn NPCs
        SpawnNPCs();

        // connect up power grids
        Debug.Log("connecting power grid...");
        foreach (PoweredComponent component in GameObject.FindObjectsOfType<PoweredComponent>()) {
            if (poweredComponents.ContainsKey(component.idn)) {
                poweredComponents[component.idn].Add(component);
            } else {
                poweredComponents[component.idn] = new HashSet<PoweredComponent> { component };
            }
        }

        // connect up cyber grids
        Debug.Log("connecting cyber grid...");
        foreach (CyberComponent component in GameObject.FindObjectsOfType<CyberComponent>()) {
            if (cyberComponents.ContainsKey(component.idn)) {
                cyberComponents[component.idn].Add(component);
            } else {
                cyberComponents[component.idn] = new HashSet<CyberComponent> { component };
            }
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
        alarmSound = Resources.Load(gameData.levelData.alarmAudioClipPath) as AudioClip;
        strikeTeamSpawnPoint = GameObject.FindObjectsOfType<NPCSpawnPoint>().Where(spawn => spawn.isStrikeTeamSpawn).First();

        OnSuspicionChange?.Invoke();
    }

    // TODO: this belongs to level logic, but it's fine to put it here for now
    public void SetNodeEnabled<T, U>(T graphNodeComponent, bool state) where T : GraphNodeComponent<T, U> where U : Node {
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
            node.enabled = state;
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
                    RefreshAlarmGraph();
                    break;
            };
        } else {
            // Debug.Log("called set node enabled with null node");
        }
    }

    // TODO: these methods could belong to the graphs?
    public CyberNode GetCyberNode(string idn) {
        return gameData?.levelData?.cyberGraph?.nodes.ContainsKey(idn) ?? false ? gameData.levelData.cyberGraph.nodes[idn] : null;
    }
    public PowerNode GetPowerNode(string idn) {
        return gameData?.levelData?.powerGraph?.nodes.ContainsKey(idn) ?? false ? gameData.levelData.powerGraph.nodes[idn] : null;
    }
    public AlarmNode GetAlarmNode(string idn) {
        return gameData?.levelData?.alarmGraph?.nodes.ContainsKey(idn) ?? false ? gameData.levelData.alarmGraph.nodes[idn] : null;
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
            return poweredComponents[idn].First();
        } else return null;
    }
    public CyberComponent GetCyberComponent(string idn) {
        if (cyberComponents == null)
            return null;
        if (cyberComponents.ContainsKey(idn)) {
            return cyberComponents[idn].First();
        } else return null;
    }
    public void SetPowerNodeState(PoweredComponent poweredComponent, bool state) {
        string idn = poweredComponent.idn;
        if (gameData.levelData != null && gameData.levelData.powerGraph != null && gameData.levelData.powerGraph.nodes.ContainsKey(idn)) {
            // Debug.Log($"setting power node power {idn} {state}");
            gameData.levelData.powerGraph.nodes[idn].powered = state;
            RefreshPowerGraph();
        }
    }
    public void SetCyberNodeState(CyberComponent cyberComponent, bool state) {
        string idn = cyberComponent.idn;
        if (gameData.levelData != null && gameData.levelData.cyberGraph != null && gameData.levelData.cyberGraph.nodes.ContainsKey(idn)) {
            SetCyberNodeState(gameData.levelData.cyberGraph.nodes[idn], state);
        }
    }
    public void SetAlarmNodeState(AlarmComponent alarmComponent, bool state) {
        string idn = alarmComponent.idn;
        if (gameData.levelData != null && gameData.levelData.alarmGraph != null && gameData.levelData.alarmGraph.nodes.ContainsKey(idn)) {
            SetAlarmNodeState(gameData.levelData.alarmGraph.nodes[idn], state);
        }
    }
    public void SetCyberNodeState(CyberNode node, bool state) {
        node.compromised = state;
        RefreshCyberGraph();
    }
    public void SetAlarmNodeState(AlarmNode node, bool state) {
        if (node.enabled) {
            node.alarmTriggered = state;
            node.countdownTimer = 30f;
            RefreshAlarmGraph();
        }
    }
    public bool IsCyberNodeVulnerable(CyberNode node) {
        if (node.compromised)
            return false;
        if (gameData.levelData != null && gameData.levelData.cyberGraph != null && gameData.levelData.cyberGraph.nodes.ContainsKey(node.idn)) {
            foreach (CyberNode neighbor in gameData.levelData.cyberGraph.Neighbors(node)) {
                if (neighbor.compromised) return true;
            }
            return false;
        } else return false;
    }

    public void RefreshCyberGraph() {
        TransferCyberState();

        gameData.levelData.cyberGraph.Refresh();

        // propagate changes to UI
        OnCyberGraphChange?.Invoke(gameData.levelData.cyberGraph);
    }
    public void RefreshAlarmGraph() {

        // determine if any active alarm object reaches a terminal
        gameData.levelData.alarmGraph.Refresh();

        TransferAlarmState();

        // propagate changes to UI
        OnAlarmGraphChange?.Invoke(gameData.levelData.alarmGraph);
    }
    public void RefreshPowerGraph() {
        // power distribution algorithm
        gameData.levelData.powerGraph.Refresh();

        // propagate the changes to local state
        TransferPowerState();

        // propagate changes to UI
        OnPowerGraphChange?.Invoke(gameData.levelData.powerGraph);
    }
    void TransferPowerState() {
        if (poweredComponents == null)
            return;
        foreach (KeyValuePair<string, PowerNode> kvp in gameData.levelData.powerGraph.nodes) {
            if (poweredComponents.ContainsKey(kvp.Key)) {
                foreach (PoweredComponent component in poweredComponents[kvp.Key]) {
                    component.power = kvp.Value.powered;
                    component.nodeEnabled = kvp.Value.enabled;
                    // Debug.Log($"transfer power to {kvp.Key}: {kvp.Value.powered}");
                }
            }
        }
    }
    void TransferCyberState() {
        if (cyberComponents == null)
            return;
        foreach (KeyValuePair<string, CyberNode> kvp in gameData.levelData.cyberGraph.nodes) {
            if (cyberComponents.ContainsKey(kvp.Key)) {
                foreach (CyberComponent component in cyberComponents[kvp.Key]) {
                    component.compromised = kvp.Value.compromised;
                    component.nodeEnabled = kvp.Value.enabled;

                    // Debug.Log($"transfer power to {kvp.Key}: {kvp.Value.power}");
                }
            }
        }
    }
    void TransferAlarmState() {
        if (alarmComponents == null)
            return;
        foreach (KeyValuePair<string, AlarmNode> kvp in gameData.levelData.alarmGraph.nodes) {
            if (alarmComponents.ContainsKey(kvp.Key)) {
                AlarmComponent component = alarmComponents[kvp.Key];
                component.alarmTriggered = kvp.Value.alarmTriggered;
                component.countdownTimer = kvp.Value.countdownTimer;
                component.nodeEnabled = kvp.Value.enabled;
            }
        }
    }


    GameObject SpawnPlayer(PlayerState state) {
        PlayerSpawnPoint spawnPoint = GameObject.FindObjectOfType<PlayerSpawnPoint>();
        Debug.Log(spawnPoint);
        return spawnPoint.SpawnPlayer(state);
    }

    void SpawnNPCs() {
        foreach (NPCSpawnPoint spawnPoint in GameObject.FindObjectsOfType<NPCSpawnPoint>().Where(spawn => !spawn.isStrikeTeamSpawn)) {
            Debug.Log($"spawning {spawnPoint}");
            spawnPoint.SpawnNPC();
        }
    }
}