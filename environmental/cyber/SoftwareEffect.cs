using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
[System.Serializable]
public class SoftwareEffect {
    public enum Type { scanAll, download, unlock, compromise, none, scanNode, scanEdges, scanFile }
    public Type type;
    public string DescriptionString() {
        if (type == Type.none) {
            return "no effect";
        } else {
            return $"<b>{TitleString()}</b>: {JustDescription()}";
        }
    }
    public string TitleString() => type switch {
        Type.scanAll => "scan",
        Type.scanNode => "scan node",
        Type.scanEdges => "scan edges",
        Type.scanFile => "scan data",
        Type.download => "download",
        Type.unlock => "crack",
        Type.compromise => "exploit",
        _ => "no effect"
    };

    public string JustDescription() {
        switch (type) {
            case Type.scanAll:
                return "reveal the node type, neighbors, and data";
            case Type.scanNode:
                return "reveal the node type";
            case Type.scanEdges:
                return "reveal the node neighbors";
            case Type.scanFile:
                return "reveal the node data";
            case Type.download:
                return "download the node data";
            case Type.unlock:
                return "crack the password for the node";
            case Type.compromise:
                return "take control of the node";
            default:
                return "no effect";
        }
    }
    public void ApplyToNode(CyberNode node, CyberGraph graph) {
        switch (type) {
            case Type.scanAll:
                GameManager.I.DiscoverNode(node, NodeVisibility.known, discoverEdges: true, discoverFile: true);
                break;
            case Type.scanNode:
                GameManager.I.DiscoverNode(node, NodeVisibility.known);
                break;
            case Type.scanEdges:
                GameManager.I.DiscoverNode(node, discoverEdges: true);
                break;
            case Type.scanFile:
                GameManager.I.DiscoverNode(node, discoverFile: true);
                break;
            case Type.download:
                if (node.type == CyberNodeType.datanode) {
                    node.DownloadData();
                }
                break;
            case Type.unlock:
                node.lockLevel = 0;
                node.datafileVisibility = true;
                if (node.visibility == NodeVisibility.unknown || node.visibility == NodeVisibility.mystery) {
                    node.visibility = NodeVisibility.known;
                }
                break;
            case Type.compromise:
                node.compromised = true;
                break;
        }
    }

    public int CalculateDesignPoints() => type switch {
        Type.scanAll => 4,
        Type.scanNode => 1,
        Type.scanEdges => 1,
        Type.scanFile => 1,
        Type.download => 1,
        Type.unlock => 6,
        Type.compromise => 8,
        _ => 1
    };

    public int CalculateSize() => type switch {
        Type.scanAll => 2,
        Type.scanNode => 1,
        Type.scanEdges => 1,
        Type.scanFile => 1,
        Type.download => 1,
        Type.unlock => 2,
        Type.compromise => 3,
        _ => 1
    };
}