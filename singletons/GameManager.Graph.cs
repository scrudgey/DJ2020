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
    // these things should belong to level delta
    public static Dictionary<string, PoweredComponent> poweredComponents;
    public static Dictionary<string, CyberComponent> cyberComponents;
    public static Dictionary<string, AlarmComponent> alarmComponents;

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
                    RefreshAlarmGraph();
                    break;
            };
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
    public void SetCyberNodeCompromised(CyberComponent cyberComponent, bool state) {
        if (applicationIsQuitting) return;
        CyberNode node = GetCyberNode(cyberComponent.idn);
        if (node != null) {
            SetCyberNodeCompromised(node, state);
        }
    }
    public void SetCyberNodeCompromised(CyberNode node, bool state) {
        if (applicationIsQuitting) return;
        node.compromised = state;
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
    public void SetAlarmOverride(AlarmComponent component) {
        string idn = component.idn;
        AlarmNode node = GetAlarmNode(idn);
        if (node != null) {
            node.overrideState = AlarmNode.AlarmOverrideState.disabled;
            RefreshAlarmGraph();
        }
    }
    public bool GetAlarmNodeTriggered(AlarmComponent alarmComponent) {
        if (applicationIsQuitting) return false;
        return GetAlarmNode(alarmComponent.idn)?.alarmTriggered ?? false;
    }





    /* REFRESH */
    public void RefreshCyberGraph() {
        if (applicationIsQuitting) return;
        if (gameData.levelState.delta.cyberGraph == null) return;

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




    void TransferPowerStateFromGraphToComponents() {
        foreach (KeyValuePair<string, PowerNode> kvp in gameData.levelState.delta.powerGraph.nodes) {
            GetPowerComponent(kvp.Key)?.ApplyNodeState(kvp.Value);
        }
    }
    void TransferCyberStateFromGraphToComponents() {
        foreach (KeyValuePair<string, CyberNode> kvp in gameData.levelState.delta.cyberGraph.nodes) {
            GetCyberComponent(kvp.Key)?.ApplyNodeState(kvp.Value);
        }
    }
    void TransferAlarmStateFromGraphToComponents() {
        foreach (KeyValuePair<string, AlarmNode> kvp in gameData.levelState.delta.alarmGraph.nodes) {
            GetAlarmComponent(kvp.Key)?.ApplyNodeState(kvp.Value);
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
    public AlarmComponent GetAlarmComponent(string idn) {
        return alarmComponents?.ContainsKey(idn) ?? false ? alarmComponents[idn] : null;
    }
    public PoweredComponent GetPowerComponent(string idn) {
        return poweredComponents?.ContainsKey(idn) ?? false ? poweredComponents[idn] : null;
    }
    public CyberComponent GetCyberComponent(string idn) {
        return cyberComponents?.ContainsKey(idn) ?? false ? cyberComponents[idn] : null;
    }
}