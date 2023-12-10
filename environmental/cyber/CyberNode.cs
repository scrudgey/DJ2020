using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class CyberNode : Node<CyberNode> {
    public bool compromised;
    public PayData payData;
    public CyberNodeType type;
    public CyberNode() { }
}


public enum CyberNodeType { normal, datanode, utility }