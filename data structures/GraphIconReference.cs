using System.Collections;
using System.Collections.Generic;
using Nimrod;
using UnityEngine;
[CreateAssetMenu(menuName = "ScriptableObjects/GraphIconReference")]
public class GraphIconReference : ScriptableObject {
    [Header("cyber sprites")]
    public Sprite normalIcon;
    public Sprite wanIcon;
    public Sprite datastoreIcon;
    public Sprite utilityIcon;
    public Sprite mysteryIcon;
    public Sprite lockIcon;
    [Header("power sprites")]
    public Sprite normalPowerIcon;
    public Sprite powerSourceIcon;
    [Header("alarm sprites")]
    public Sprite normalAlarmIcon;
    public Sprite alarmTerminalIcon;
    public Sprite alarmRadioIcon;
    [Header("datatype icons")]
    public Sprite iconPay;
    public Sprite iconPersonnel;
    public Sprite iconPassword;
    public Sprite iconLocation;
    public Sprite iconObjective;
    public Sprite iconGenericData;
    [Header("colors")]
    public Color minimapCyberColor;
    public Color minimapAlarmColor;
    public Color minimapPowerColor;
    [Header("keyImages")]
    public Sprite physicalKey;
    public Sprite keyCard;
    public Sprite password;

    public Sprite CyberNodeSprite(CyberNode node) {
        // if (node.lockLevel > 0) {
        //     return lockIcon;
        // }
        if (node.visibility < NodeVisibility.known) {
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
    public Sprite PowerNodeSprite(PowerNode node) {
        if (node.visibility < NodeVisibility.known) {
            return mysteryIcon;
        }
        switch (node.type) {
            default:
            case PowerNode.NodeType.none:
                return normalPowerIcon;
            case PowerNode.NodeType.powerSource:
                return powerSourceIcon;
        }
    }

    public Sprite AlarmNodeSprite(AlarmNode node) {
        if (node.visibility < NodeVisibility.known) {
            return mysteryIcon;
        }
        switch (node.nodeType) {
            default:
            case AlarmNode.AlarmNodeType.normal:
                return normalAlarmIcon;
            case AlarmNode.AlarmNodeType.terminal:
                return alarmTerminalIcon;
            case AlarmNode.AlarmNodeType.radio:
                return alarmRadioIcon;
        }
    }

    public Sprite DataSprite(PayData.DataType dataType) {
        return dataType switch {
            PayData.DataType.location => iconLocation,
            PayData.DataType.objective => iconObjective,
            PayData.DataType.password => iconPassword,
            PayData.DataType.pay => iconPay,
            PayData.DataType.personnel => iconPersonnel,
            PayData.DataType.unknown => iconGenericData,
            _ => iconPay
        };
    }
}
