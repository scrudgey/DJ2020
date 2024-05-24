using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;

[System.Serializable]
public class CyberGraph : Graph<CyberNode, CyberGraph> {

    [System.NonSerialized]
    [XmlIgnore]
    public Action<Dictionary<CyberNode, List<NetworkAction>>> NetworkActionsChanged;
    // [System.NonSerialized]
    // [XmlIgnore]
    // public Action<NetworkAction> NetworkActionUpdate;
    [System.NonSerialized]
    [XmlIgnore]
    public Action<NetworkAction> NetworkActionComplete;
    [System.NonSerialized]
    [XmlIgnore]
    public Dictionary<CyberNode, List<NetworkAction>> networkActions;
    [System.NonSerialized]
    [XmlIgnore]
    public List<VirusProgram> virusPrograms;

    [System.NonSerialized]
    [XmlIgnore]
    public Action<VirusProgram> OnAddVirusProgram;
    [System.NonSerialized]
    [XmlIgnore]
    public Action<VirusProgram> OnRemoveVirusProgram;
    public bool IsCyberNodeVulnerable(CyberNode node) {
        if (node.compromised)
            return false;
        if (nodes.ContainsKey(node.idn)) {
            foreach (CyberNode neighbor in Neighbors(node)) {
                if (neighbor.compromised) return true;
            }
            return false;
        } else return false;
    }

    public CyberNodeStatus GetStatus(CyberNode node) {
        if (node.compromised) {
            return CyberNodeStatus.compromised;
        } else {
            if (IsCyberNodeVulnerable(node)) {
                return CyberNodeStatus.vulnerable;
            } else {
                return CyberNodeStatus.invulnerable;
            }
        }
    }

    public void Refresh() {
        foreach (CyberNode node in nodes.Values) {
            node.status = GetStatus(node);
        }
        // foreach (KeyValuePair<string, CyberNode> kvp in nodes) {
        //     if (kvp.Value.visibility == NodeVisibility.unknown) {
        //         if (edges.ContainsKey(kvp.Key) && edges[kvp.Key].Any(idn => nodes[idn].visibility == NodeVisibility.mapped)) {
        //             kvp.Value.visibility = NodeVisibility.mystery;
        //         }
        //     }
        // }
    }


    public void ApplyPayDataState(Dictionary<string, PayData> paydataData) {
        foreach (KeyValuePair<string, PayData> kvp in paydataData) {
            if (nodes.ContainsKey(kvp.Key)) {
                nodes[kvp.Key].payData = kvp.Value;
            }
        }
    }

    public void InfillRandomData() {
        foreach (KeyValuePair<string, CyberNode> kvp in nodes) {
            kvp.Value.payData = PayData.RandomPaydata();
        }
    }
    public void InfillDummyData() {
        foreach (KeyValuePair<string, CyberNode> kvp in nodes) {
            kvp.Value.payData = PayData.DummyPaydata();
        }
    }
    public void InfillDummyObjective(ObjectiveData objectiveData) {
        nodes[objectiveData.potentialSpawnPoints[0]].payData = PayData.DummyObjective(objectiveData);
    }
    public void AddNetworkAction(NetworkAction action) {
        if (networkActions == null) {
            networkActions = new Dictionary<CyberNode, List<NetworkAction>>();
        }
        if (!networkActions.ContainsKey(action.toNode)) {
            networkActions[action.toNode] = new List<NetworkAction>();
        }
        networkActions[action.toNode].Add(action);
        NetworkActionsChanged?.Invoke(networkActions);
    }
    public void AddVirusProgram(VirusProgram virus) {
        if (virusPrograms == null) {
            virusPrograms = new List<VirusProgram>();
        }
        virusPrograms.Add(virus);
        OnAddVirusProgram?.Invoke(virus);
    }
    public void UpdateNetworkActions(float timeDelta) {
        bool anyComplete = false;
        if (networkActions == null) {
            networkActions = new Dictionary<CyberNode, List<NetworkAction>>();
        }
        if (virusPrograms == null) {
            virusPrograms = new List<VirusProgram>();
        }

        // update network actions
        foreach (List<NetworkAction> actions in networkActions.Values) {
            List<NetworkAction> completedActions = new List<NetworkAction>();
            foreach (NetworkAction action in actions) {
                action.Update(timeDelta, this);
                if (action.complete) {
                    completedActions.Add(action);
                    anyComplete = true;
                }
            }
            foreach (NetworkAction action in completedActions) {
                actions.Remove(action);
                NetworkActionComplete?.Invoke(action);
            }
        }

        // update viruses
        List<VirusProgram> completedViruses = new List<VirusProgram>();
        List<VirusProgram> newViruses = new List<VirusProgram>();
        foreach (VirusProgram virus in virusPrograms) {
            // display virus at position
            virus.Update(timeDelta);
            if (virus.updated) {
                anyComplete = true;
                virus.updated = false;
            }
            if (virus.complete) {
                anyComplete = true;
                completedViruses.Add(virus);
            }
            if (virus.duplicate) {
                virus.duplicate = false;
                newViruses.Add(virus.Duplicate());
            }
        }
        foreach (VirusProgram virus in newViruses) {
            AddVirusProgram(virus);
        }
        foreach (VirusProgram virus in completedViruses) {
            virusPrograms.Remove(virus);
            OnRemoveVirusProgram?.Invoke(virus);
        }

        if (anyComplete) {
            GameManager.I.RefreshCyberGraph();
            NetworkActionsChanged?.Invoke(networkActions);
        }
    }

    public List<NetworkAction> ActiveNetworkActions() {
        List<NetworkAction> output = new List<NetworkAction>();
        if (networkActions == null) return output;
        foreach (KeyValuePair<CyberNode, List<NetworkAction>> kvp in networkActions) {
            output.AddRange(kvp.Value);
        }
        return output;
    }
    public IEnumerable<List<CyberNode>> ActiveNetworkPaths() {
        return networkActions.Values.SelectMany(networkActionList => networkActionList.Select(act => act.path));
    }

    public CyberNode GetNearestCompromisedNode(CyberNode origin) {
        foreach (String neighborId in edges[origin.idn]) {
            CyberNode neighbor = nodes[neighborId];
            if (neighbor.getStatus() == CyberNodeStatus.compromised) {
                return neighbor;
            }
        }
        return null;
    }

    public List<CyberNode> GetPathToNearestDownloadPoint(CyberNode origin) {
        HashSet<string> pathids = DFS(origin, new HashSet<string>(), new HashSet<string>(),
            (CyberNode node) => node.dataSink);
        List<CyberNode> output = new List<CyberNode>();
        foreach (string idn in pathids) {
            output.Add(nodes[idn]);
        }
        return output;
    }
    public List<CyberNode> GetPathToNearestCompromised(CyberNode origin) {
        HashSet<string> pathids = DFS(origin, new HashSet<string>(), new HashSet<string>(),
            (CyberNode node) => node.getStatus() == CyberNodeStatus.compromised);
        List<CyberNode> output = new List<CyberNode>();
        foreach (string idn in pathids) {
            output.Add(nodes[idn]);
        }
        return output;
    }
    public List<CyberNode> GetPath(CyberNode origin, CyberNode destination) {
        HashSet<string> pathids = DFS(origin, new HashSet<string>(), new HashSet<string>(),
            (CyberNode node) => node == destination);
        List<CyberNode> output = new List<CyberNode>();
        foreach (string idn in pathids) {
            output.Add(nodes[idn]);
        }
        return output;
    }

    HashSet<string> DFS(CyberNode node, HashSet<string> visitedNodes, HashSet<string> path, Func<CyberNode, bool> targetEvaluator) {
        if (visitedNodes.Contains(node.idn)) {
            return new HashSet<string>();
        } else {
            visitedNodes.Add(node.idn);
        }

        path.Add(node.idn);
        if (targetEvaluator(node)) {
            return path;
        }

        if (edges.ContainsKey(node.idn))
            foreach (string neighborID in edges[node.idn]) {
                if (visitedNodes.Contains(neighborID)) {
                    continue;
                }

                CyberNode neighborNode = nodes[neighborID];
                if (!neighborNode.getEnabled()) {
                    continue;
                    // } else if (node.visibility != NodeVisibility.mapped && neighborNode.visibility != NodeVisibility.mapped) {
                } else if (edgeVisibility[(node.idn, neighborID)] == EdgeVisibility.unknown) {
                    // neither node is mapped- edge is unknwon
                    continue;
                }

                HashSet<string> newNodes = visitedNodes.ToHashSet();
                HashSet<string> newPath = path.ToHashSet();
                HashSet<string> output = DFS(neighborNode, newNodes, newPath, targetEvaluator);
                if (output.Count == 0) {
                    continue;
                } else {
                    return output;
                }
            }
        return new HashSet<string>();
    }


    public override bool DiscoverNode(CyberNode node,
                                NodeVisibility newNodeVisibility = NodeVisibility.unknown,
                                bool discoverEdges = false,
                                bool discoverFile = false) {
        bool value = base.DiscoverNode(node, newNodeVisibility, discoverEdges, discoverFile);
        if (discoverFile && !node.datafileVisibility) {
            value = true;
            node.datafileVisibility = true;
        }
        return value;
    }
}
