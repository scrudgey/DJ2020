using System.Collections;
using System.Collections.Generic;
using Nimrod;
using UnityEngine;
[CreateAssetMenu(menuName = "ScriptableObjects/GraphIconReference")]
public class GraphIconReference : ScriptableObject {
    [Header("sprites")]
    public Sprite normalIcon;
    public Sprite wanIcon;
    public Sprite datastoreIcon;
    public Sprite utilityIcon;
    public Sprite mysteryIcon;

    public Sprite CyberNodeSprite(CyberNode node) {
        if (node.visibility == NodeVisibility.mystery) {
            return mysteryIcon;
        }
        switch (node.type) {
            default:
            case CyberNodeType.normal:
                return normalIcon;
            case CyberNodeType.datanode:
                return datastoreIcon;
            case CyberNodeType.utility:
                return utilityIcon;
            case CyberNodeType.WAN:
                return wanIcon;
        }
    }
}
