using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
public partial class GameManager : Singleton<GameManager> {

    static Dictionary<string, HashSet<PoweredComponent>> poweredComponents;
    static Dictionary<string, HashSet<CyberComponent>> cyberComponents;
    static Dictionary<string, AlarmComponent> alarmComponents;

    Transform strikeTeamSpawnPoint;

    public static class Level {
        public static string LevelDataPath(string levelName) {
            string path = Path.Combine(Application.dataPath, "Resources", "data", "levels", levelName);
            if (!Directory.Exists(path)) {
                Directory.CreateDirectory(path);
            }
            return path;
        }
    }

    public void SetFocus(GameObject focus) {
        this.playerObject = focus;
        this.playerLightLevelProbe = focus.GetComponentInChildren<LightLevelProbe>();
        this.playerGunHandler = focus.GetComponentInChildren<GunHandler>();
        this.playerItemHandler = focus.GetComponentInChildren<ItemHandler>();
        this.playerInteractor = focus.GetComponentInChildren<Interactor>();

        if (playerOutlineHandler != null) {
            playerOutlineHandler.UnBind();
        }
        playerOutlineHandler = focus.GetComponentInChildren<PlayerOutlineHandler>();
        playerOutlineHandler?.Bind();

        Debug.Log("setting focus...");
        // if (OnFocusChanged != null) {
        OnFocusChanged?.Invoke(focus);
        // }
    }
    void ClearSceneData() {
        poweredComponents = new Dictionary<string, HashSet<PoweredComponent>>();
        cyberComponents = new Dictionary<string, HashSet<CyberComponent>>();
        alarmComponents = new Dictionary<string, AlarmComponent>();
    }
    private void InitializeLevel(string levelName) {
        // TODO: level enum input
        // TODO: load and set up asynchronously behind a screen
        ClearSceneData();

        GameObject playerObj = GameObject.Find("playerCharacter");
        SetFocus(playerObj);

        // load global state
        Debug.Log($"loading level data {levelName}...");
        gameData.levelData = LevelData.LoadLevelData(levelName);

        // load scene state
        // TODO: load not just player but all saveable objects
        Debug.Log($"loading player state...");
        LoadPlayerState(gameData.playerData);

        // TODO: abstract this
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

        // connect up cyber grids
        Debug.Log("connecting alarm grid...");
        foreach (AlarmComponent component in GameObject.FindObjectsOfType<AlarmComponent>()) {
            alarmComponents[component.idn] = component;
        }

        // TODO: abstract this?
        RefreshPowerGraph();
        RefreshCyberGraph();
        RefreshAlarmGraph();


        PoolManager.I.RegisterPool("prefabs/NPC", poolSize: 6);
        alarmSoundInterval = 2f;
        // initialize spawn point
        strikeTeamSpawnPoint = GameObject.Find("strikeTeamSpawnPoint")?.transform;
        alarmSound = Resources.Load(gameData.levelData.alarmAudioClipPath) as AudioClip;
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
        if (alarmComponents.ContainsKey(idn)) {
            return alarmComponents[idn];
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
    public void SetCyberNodeState(CyberNode node, bool state) {
        node.compromised = state;
        RefreshCyberGraph();
    }
    public void SetAlarmNodeState(AlarmNode node, bool state) {
        node.alarmTriggered = state;
        node.countdownTimer = 30f;
        RefreshAlarmGraph();
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
        foreach (KeyValuePair<string, PowerNode> kvp in gameData.levelData.powerGraph.nodes) {
            if (poweredComponents.ContainsKey(kvp.Key)) {
                foreach (PoweredComponent component in poweredComponents[kvp.Key]) {
                    component.power = kvp.Value.powered;
                    // Debug.Log($"transfer power to {kvp.Key}: {kvp.Value.powered}");
                }
            }
        }
    }
    void TransferCyberState() {
        foreach (KeyValuePair<string, CyberNode> kvp in gameData.levelData.cyberGraph.nodes) {
            if (cyberComponents.ContainsKey(kvp.Key)) {
                foreach (CyberComponent component in cyberComponents[kvp.Key]) {
                    component.compromised = kvp.Value.compromised;
                    // Debug.Log($"transfer power to {kvp.Key}: {kvp.Value.power}");
                }
            }
        }
    }
    void TransferAlarmState() {
        foreach (KeyValuePair<string, AlarmNode> kvp in gameData.levelData.alarmGraph.nodes) {
            if (alarmComponents.ContainsKey(kvp.Key)) {
                AlarmComponent component = alarmComponents[kvp.Key];
                component.alarmTriggered = kvp.Value.alarmTriggered;
                component.countdownTimer = kvp.Value.countdownTimer;
            }
        }
    }
    public void LoadPlayerState(PlayerData data) {
        foreach (ISaveable saveable in playerObject.GetComponentsInChildren<ISaveable>()) {
            // Debug.Log("triggering load on " + saveable);
            saveable.LoadState(data);
        }
    }
}