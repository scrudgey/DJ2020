using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
[System.Serializable]
public class SoftwareEffect {
    public enum Type { scan, download, unlock, compromise, none, scanNode, scanEdges, scanFile }
    public Type type;
    public int level;
    public string name;
    public string DescriptionString() {
        switch (type) {
            case Type.scan:
                return "<b>scan</b>: reveal the node type, neighbors, and data";
            case Type.scanNode:
                return "<b>scan node</b>: reveal the node type";
            case Type.scanEdges:
                return "<b>scan edges</b>: reveal the node neighbors";
            case Type.scanFile:
                return "<b>scan data</b>: reveal the node data";
            case Type.download:
                return "<b>download</b>: download the node data";
            case Type.unlock:
                return "<b>crack</b>: crack the password for the node";
            case Type.compromise:
                return "<b>exploit</b>: take control of the node";
            default:
                return "no effect";
        }
    }
    public void ApplyToNode(CyberNode node, CyberGraph graph) {
        switch (type) {
            case Type.scan:
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
                if (node.visibility == NodeVisibility.unknown || node.visibility == NodeVisibility.mystery) {
                    node.visibility = NodeVisibility.known;
                }
                break;
            case Type.compromise:
                node.compromised = true;
                break;
        }
    }
}