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
    public Sprite lockIcon;
    [Header("datatype icons")]
    public Sprite iconPay;
    public Sprite iconPersonnel;
    public Sprite iconPassword;
    public Sprite iconLocation;
    public Sprite iconObjective;

    public Sprite CyberNodeSprite(CyberNode node) {
        if (node.lockLevel > 0) {
            return lockIcon;
        }
        if (node.visibility == NodeVisibility.mystery) {
            return mysteryIcon;
        }
        switch (node.type) {
            default:
            case CyberNodeType.normal:
                return normalIcon;
            case CyberNodeType.datanode:
                return DataSprite(node.payData.type);
            case CyberNodeType.utility:
                return utilityIcon;
            case CyberNodeType.WAN:
                return wanIcon;
        }
    }

    public Sprite DataSprite(PayData.DataType dataType) {
        return dataType switch {
            PayData.DataType.location => iconLocation,
            PayData.DataType.objective => iconObjective,
            PayData.DataType.password => iconPassword,
            PayData.DataType.pay => iconPay,
            PayData.DataType.personnel => iconPersonnel,
            _ => iconPay
        };
    }
}
