using System.Collections.Generic;
using System.IO;
using UnityEngine;
public partial class GameManager : Singleton<GameManager> {

    Dictionary<string, HashSet<PoweredComponent>> poweredComponents;

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
        RefreshPowerGraph();
    }

    // TODO: this belongs to level logic, but it's fine to put it here for now
    public void SetPowerSourceState(string idn, bool state) {
        gameData.levelData.powerGraph.nodes[idn].powerSource = state;
        RefreshPowerGraph();
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
                    component.power = kvp.Value.power;
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