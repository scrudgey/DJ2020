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
    public void UpdateGraphs() {
        // method for time-updating graph state

        float alarmTimerOrig = alarmCountdown();

        gameData?.levelState?.delta.alarmGraph?.Update();

        float alarmTimer = alarmCountdown();
        if (alarmTimer <= 0 && alarmTimerOrig > 0) {
            InitiateAlarmShutdown();
        }

        gameData?.levelState?.delta.cyberGraph?.UpdateNetworkActions(Time.deltaTime);
    }
    public void AddNetworkAction(NetworkAction networkAction) {
        gameData.levelState.delta.cyberGraph.AddNetworkAction(networkAction);
        RefreshCyberGraph();
    }
    public void SetNodeEnabled<U>(Node<U> node, bool value) where U : Node<U> {
        if (applicationIsQuitting) return;
        node.setEnabled(value);
        switch (node) {
            case CyberNode:
                RefreshCyberGraph();
                break;
            case PowerNode:
                RefreshPowerGraph();
                break;
            case AlarmNode:
                RefreshAlarmGraph();
                break;
        }
    }

    /* POWER NODE */
    public void SetPowerNodeState(PoweredComponent poweredComponent, bool state) {
        if (applicationIsQuitting) return;
        string idn = poweredComponent.idn;
        if (gameData.levelState != null && gameData.levelState.delta.powerGraph != null && gameData.levelState.delta.powerGraph.nodes.ContainsKey(idn)) {
            gameData.levelState.delta.powerGraph.nodes[idn].powered = state;
            RefreshPowerGraph();
        }
    }

    /* CYBER NODE  */
    public void SetCyberNodeCompromised(CyberNode node, bool state) {
        if (applicationIsQuitting) return;
        node.compromised = state;
        RefreshCyberGraph();
    }
    public void SetCyberNodeUtilityState(CyberNode node, bool state) {
        if (applicationIsQuitting) return;
        node.utilityActive = state;
        RefreshCyberGraph();
    }
    public bool IsCyberNodeVulnerable(CyberNode node) {
        return gameData.levelState?.delta.cyberGraph?.IsCyberNodeVulnerable(node) ?? false;
    }

    /* ALARM NODE */
    public void SetAlarmNodeTriggered(AlarmComponent alarmComponent, bool state) {
        if (applicationIsQuitting) return;
        AlarmNode node = GetAlarmNode(alarmComponent.idn);
        if (node != null) {
            SetAlarmNodeTriggered(node, state);
        }
    }
    public void SetAlarmNodeTriggered(AlarmNode node, bool state) {
        if (applicationIsQuitting) return;
        if (node == null) return;
        if (state && node.getEnabled()) {
            node.alarmTriggered = true;
            node.countdownTimer = 30f;
        } else {
            node.alarmTriggered = false;
            node.countdownTimer = 0f;
        }
        RefreshAlarmGraph();
    }
    public bool GetAlarmNodeTriggered(AlarmComponent alarmComponent) {
        if (applicationIsQuitting) return false;
        return GetAlarmNode(alarmComponent.idn)?.alarmTriggered ?? false;
    }





    /* REFRESH */
    public void RefreshCyberGraph() {
        if (applicationIsQuitting) return;
        if (gameData.levelState.delta.cyberGraph == null) return;

        gameData.levelState.delta.cyberGraph.Refresh();

        // transfer from state -> components
        TransferCyberStateFromGraphToComponents();

        // propagate changes to UI
        OnCyberGraphChange?.Invoke(gameData.levelState.delta.cyberGraph);
    }
    public void RefreshAlarmGraph() {
        if (applicationIsQuitting) return;
        if (gameData.levelState.delta.alarmGraph == null) return;

        // determine if any active alarm object reaches a terminal
        gameData.levelState.delta.alarmGraph.Refresh();

        TransferAlarmStateFromGraphToComponents();

        // propagate changes to UI
        OnAlarmGraphChange?.Invoke(gameData.levelState.delta.alarmGraph);

        if (gameData.levelState.anyAlarmTerminalActivated()) {
            ActivateLevelAlarm();
        } else {
            DeactivateAlarm();
        }
    }
    public void RefreshPowerGraph() {
        if (applicationIsQuitting) return;
        if (gameData.levelState.delta.powerGraph == null) return;

        // power distribution algorithm
        gameData.levelState.delta.powerGraph.Refresh();

        // propagate the changes to local state
        TransferPowerStateFromGraphToComponents();

        // propagate changes to UI
        OnPowerGraphChange?.Invoke(gameData.levelState.delta.powerGraph);
    }



    // TODO: remove these functions?
    void TransferPowerStateFromGraphToComponents() {
        foreach (KeyValuePair<string, PowerNode> kvp in gameData.levelState.delta.powerGraph.nodes) {
            kvp.Value.ValueChanged();
        }
    }
    void TransferCyberStateFromGraphToComponents() {
        foreach (KeyValuePair<string, CyberNode> kvp in gameData.levelState.delta.cyberGraph.nodes) {
            kvp.Value.ValueChanged();
        }
    }
    void TransferAlarmStateFromGraphToComponents() {
        foreach (KeyValuePair<string, AlarmNode> kvp in gameData.levelState.delta.alarmGraph.nodes) {
            kvp.Value.ValueChanged();
        }
    }
    public CyberNode GetCyberNode(string idn) {
        return gameData?.levelState?.delta.cyberGraph?.GetNode(idn) ?? null;
    }
    public PowerNode GetPowerNode(string idn) {
        return gameData?.levelState?.delta.powerGraph?.GetNode(idn) ?? null;
    }
    public AlarmNode GetAlarmNode(string idn) {
        return gameData?.levelState?.delta.alarmGraph?.GetNode(idn) ?? null;
    }

    public GameObject GetNodeComponent(string idn) {
        if (idn == "localhost") {
            return playerObject;
        }
        PoweredComponent[] poweredComponents = FindObjectsOfType<PoweredComponent>();
        PoweredComponent poweredComponent = poweredComponents.Where(component => component.idn == idn).FirstOrDefault();
        if (poweredComponent != null) {
            return poweredComponent.gameObject;
        }
        CyberComponent[] cyberComponents = FindObjectsOfType<CyberComponent>();
        CyberComponent cyberComponent = cyberComponents.Where(component => component.idn == idn).FirstOrDefault();
        if (cyberComponent != null) {
            return cyberComponent.gameObject;
        }
        AlarmComponent[] alarmComponents = FindObjectsOfType<AlarmComponent>();
        AlarmComponent alarmComponent = alarmComponents.Where(component => component.idn == idn).FirstOrDefault();
        if (alarmComponent != null) {
            return alarmComponent.gameObject;
        }
        return null;
    }

    public void DiscoverNode<U>(U node,
                                NodeVisibility newNodeVisibility = NodeVisibility.unknown,
                                bool discoverEdges = false,
                                bool discoverFile = false) where U : Node<U> {
        bool doSfx = node switch {
            CyberNode cyberNode => gameData.levelState.delta.cyberGraph.DiscoverNode(cyberNode, newNodeVisibility, discoverEdges, discoverFile),
            PowerNode powerNode => gameData.levelState.delta.powerGraph.DiscoverNode(powerNode, newNodeVisibility, discoverEdges, discoverFile),
            AlarmNode alarmNode => gameData.levelState.delta.alarmGraph.DiscoverNode(alarmNode, newNodeVisibility, discoverEdges, discoverFile)
        };

        if (doSfx) {
            Toolbox.RandomizeOneShot(audioSource, discoverySound);
            discoveryParticlePool.GetObject(node.position);
        }
    }
}