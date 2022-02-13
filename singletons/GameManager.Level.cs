using System.Collections.Generic;
using System.IO;
using UnityEngine;
public partial class GameManager : Singleton<GameManager> {

    static Dictionary<string, HashSet<PoweredComponent>> poweredComponents;
    static Dictionary<string, HashSet<CyberComponent>> cyberComponents;

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
        OnFocusChanged?.Invoke(focus);
    }
    void ClearSceneData() {
        poweredComponents = new Dictionary<string, HashSet<PoweredComponent>>();
        cyberComponents = new Dictionary<string, HashSet<CyberComponent>>();
    }
    private void InitializeLevel(string levelName) {
        // TODO: level enum input
        // TODO: load and set up asynchronously behind a screen

        ClearSceneData();

        Debug.Log("setting focus...");
        SetFocus(GameObject.Find("playerCharacter"));

        // load global state
        Debug.Log($"loading level data {levelName}...");
        gameData.levelData = LevelData.LoadLevelData(levelName);

        // load scene state
        // TODO: load not just player but all saveable objects
        Debug.Log($"loading player state...");
        LoadPlayerState(gameData.playerData);

        // connect up power grids
        Debug.Log("connecting power grid...");
        foreach (PoweredComponent component in GameObject.FindObjectsOfType<PoweredComponent>()) {
            if (poweredComponents.ContainsKey(component.idn)) {
                poweredComponents[component.idn].Add(component);
            } else {
                poweredComponents[component.idn] = new HashSet<PoweredComponent> { component };
            }
        }

        // TODO: abstract this
        // connect up cyber grids
        Debug.Log("connecting cyber grid...");
        foreach (CyberComponent component in GameObject.FindObjectsOfType<CyberComponent>()) {
            if (cyberComponents.ContainsKey(component.idn)) {
                cyberComponents[component.idn].Add(component);
            } else {
                cyberComponents[component.idn] = new HashSet<CyberComponent> { component };
            }
        }

        // TODO: abstract this?
        RefreshPowerGraph();
        RefreshCyberGraph();
    }

    // TODO: this belongs to level logic, but it's fine to put it here for now
    public void SetNodeEnabled<T, U>(T graphNodeComponent, bool state) where T : GraphNodeComponent<T, U> where U : Node {
        string idn = graphNodeComponent.idn;
        switch (graphNodeComponent) {
            case PoweredComponent:
                if (gameData.levelData != null && gameData.levelData.powerGraph != null && gameData.levelData.powerGraph.nodes.ContainsKey(idn)) {
                    gameData.levelData.powerGraph.nodes[idn].enabled = state;
                    RefreshPowerGraph();
                }
                break;
            case CyberComponent:
                if (gameData.levelData != null && gameData.levelData.cyberGraph != null && gameData.levelData.cyberGraph.nodes.ContainsKey(idn)) {
                    gameData.levelData.cyberGraph.nodes[idn].enabled = state;
                    RefreshCyberGraph();
                }
                break;
        };
    }
    public void SetPowerNodeState(PoweredComponent poweredComponent, bool state) {
        string idn = poweredComponent.idn;
        if (gameData.levelData != null && gameData.levelData.powerGraph != null && gameData.levelData.powerGraph.nodes.ContainsKey(idn)) {
            gameData.levelData.powerGraph.nodes[idn].powered = state;
            RefreshPowerGraph();
        }
    }
    public void SetCyberNodeState(CyberComponent cyberComponent, bool state) {
        Debug.Log("set cybernode state");
        string idn = cyberComponent.idn;
        if (gameData.levelData != null && gameData.levelData.cyberGraph != null && gameData.levelData.cyberGraph.nodes.ContainsKey(idn)) {
            gameData.levelData.cyberGraph.nodes[idn].compromised = state;
            RefreshCyberGraph();
        }
    }

    public void RefreshCyberGraph() {
        // TODO: abstract this
        // gameData.levelData.cyberGraph.Refresh();

        // TODO: abstract this
        TransferCyberState();

        // propagate changes to UI
        OnCyberGraphChange?.Invoke(gameData.levelData.cyberGraph);
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
                    // Debug.Log($"transfer power to {kvp.Key}: {kvp.Value.power}");
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
    public void LoadPlayerState(PlayerData data) {
        foreach (ISaveable saveable in playerObject.GetComponentsInChildren<ISaveable>()) {
            // Debug.Log("triggering load on " + saveable);
            saveable.LoadState(data);
        }
    }
}