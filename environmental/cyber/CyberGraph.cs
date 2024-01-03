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
    [System.NonSerialized]
    [XmlIgnore]
    public Action<NetworkAction> NetworkActionUpdate;
    [System.NonSerialized]
    [XmlIgnore]
    public Action<NetworkAction> NetworkActionComplete;
    [System.NonSerialized]
    [XmlIgnore]
    public Dictionary<CyberNode, List<NetworkAction>> networkActions;
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
        foreach (KeyValuePair<string, CyberNode> kvp in nodes) {
            if (kvp.Value.visibility == NodeVisibility.unknown) {
                if (edges.ContainsKey(kvp.Key) && edges[kvp.Key].Any(idn => nodes[idn].visibility == NodeVisibility.mapped)) {
                    kvp.Value.visibility = NodeVisibility.mystery;
                }
            }
        }
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

    public void ApplyObjectiveData(List<ObjectiveData> objectives) {
        List<CyberNode> dataNodes = nodes.Values.Where(node => node.type == CyberNodeType.datanode).ToList();
        foreach (ObjectiveData objective in objectives) {
            if (dataNodes.Count == 0) {
                Debug.LogError("Not enough data nodes to support level objectives! Mission is not possible.");
                break;
            }
            CyberNode target = Toolbox.RandomFromList(dataNodes);
            target.payData = objective.targetPaydata;
            dataNodes.Remove(target);
        }
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
    public void UpdateNetworkActions(float timeDelta) {
        bool anyComplete = false;
        if (networkActions == null) {
            networkActions = new Dictionary<CyberNode, List<NetworkAction>>();
        }
        foreach (List<NetworkAction> actions in networkActions.Values) {
            List<NetworkAction> completedActions = new List<NetworkAction>();
            foreach (NetworkAction action in actions) {
                action.Update(timeDelta);
                if (action.complete) {
                    completedActions.Add(action);
                    anyComplete = true;
                }
                NetworkActionUpdate?.Invoke(action);
            }
            foreach (NetworkAction action in completedActions) {
                actions.Remove(action);
                NetworkActionComplete?.Invoke(action);
            }
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
        // node.powered = true;
        HashSet<string> pathids = DFS(origin, new HashSet<HashSet<string>>(), new HashSet<string>(), new HashSet<string>());
        List<CyberNode> output = new List<CyberNode>();
        foreach (string idn in pathids) {
            output.Add(nodes[idn]);
        }
        Debug.Log($"DFS output length: {pathids.Count}");
        return output;
    }

    HashSet<string> DFS(CyberNode node, HashSet<HashSet<string>> visitedEdges, HashSet<string> visitedNodes, HashSet<string> path) {
        Debug.Log($"DFS {path.Count} -> {node.idn}");
        if (visitedNodes.Contains(node.idn)) {
            return new HashSet<string>();
        } else {
            visitedNodes.Add(node.idn);
        }
        if (edges.ContainsKey(node.idn))
            foreach (string neighborID in edges[node.idn]) {
                if (visitedNodes.Contains(neighborID)) {
                    continue;
                }
                path.Add(neighborID);
                visitedEdges.Add(new HashSet<string> { node.idn, neighborID });
                CyberNode terminalNode = nodes[neighborID];
                if (!terminalNode.getEnabled()) {
                    path.Remove(neighborID);
                    continue;
                }
                if (terminalNode.dataSink) {
                    return path;
                } else {
                    HashSet<HashSet<string>> newEdges = visitedEdges.ToHashSet();
                    HashSet<string> newNodes = visitedNodes.ToHashSet();
                    HashSet<string> newPath = path.ToHashSet();
                    HashSet<string> output = DFS(nodes[neighborID], newEdges, newNodes, newPath);
                    if (output.Count == 0) {
                        path.Remove(neighborID);
                        continue;
                    } else {
                        return output;
                    }
                }
            }
        return new HashSet<string>();
    }



    // def find_path_to_friend(network, src, dest):
    //     queue = [src]
    //     visited = defaultdict(bool)
    //     path = []
    //     while len(queue) != 0:
    //       node = queue.pop(0)
    //       path.append(node)
    //       visited[node] = True
    //       if node in network:
    //         for adj in network[node]:
    //           if adj == dest:
    //             return path + [dest]
    //           if not visited[adj]:
    //             queue.append(adj)
    //     return None

}
