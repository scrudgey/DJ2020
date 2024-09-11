using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class NodeDataInfoDisplay : MonoBehaviour {
    public Image icon;
    public TextMeshProUGUI filename;
    public TextMeshProUGUI dataType;
    public TextMeshProUGUI valueamount;
    public TextMeshProUGUI dashSpacer;

    public GameObject creditIndicator;
    public GraphIconReference iconReference;
    public Button downloadButton;
    CyberNode node;

    public void Configure(CyberNode node) {
        this.node = node;
        int numberDownloads = GameManager.I.gameData.levelState.delta.cyberGraph.networkActions
            .Values
            .SelectMany(x => x)
            .Select(x => x.softwareTemplate.principalType == SoftwareEffect.Type.download)
            .Count();

        bool nodeDownloadInProgress = false;
        if (GameManager.I.gameData.levelState.delta.cyberGraph.networkActions.ContainsKey(node)) {
            nodeDownloadInProgress = GameManager.I.gameData.levelState.delta.cyberGraph.networkActions[node].Any(x => x.softwareTemplate.principalType == SoftwareEffect.Type.download);
        }
        if (node.datafileVisibility) {
            if (node.dataStolen) {
                ConfigureStolen();
                downloadButton.gameObject.SetActive(false);
            } else {
                ConfigureNormal(node.payData);
                downloadButton.gameObject.SetActive(true);
                downloadButton.interactable = node.lockLevel == 0 && !nodeDownloadInProgress;
            }
        } else {
            ConfigureUnknown();
            downloadButton.interactable = false;
            downloadButton.gameObject.SetActive(false);
        }
    }
    void ConfigureUnknown() {
        icon.sprite = iconReference.iconDataUnknown;
        filename.text = "???";
        dataType.text = "";
        valueamount.text = "";
        creditIndicator.SetActive(false);
        valueamount.gameObject.SetActive(false);
        icon.enabled = true;
        dashSpacer.text = "";
    }
    void ConfigureStolen() {
        filename.text = "empty";
        dataType.text = $"";
        valueamount.text = $"-";
        creditIndicator.SetActive(false);
        valueamount.gameObject.SetActive(false);
        icon.sprite = null;
        icon.enabled = false;
        dashSpacer.text = "";
    }
    void ConfigureNormal(PayData data) {
        filename.text = data.filename;
        icon.enabled = true;
        icon.sprite = iconReference.DataSprite(data.type, true);
        dataType.text = data.type switch {
            PayData.DataType.pay => "value",
            PayData.DataType.location => "map data",
            PayData.DataType.objective => "mission objective",
            PayData.DataType.password => "password data",
            PayData.DataType.personnel => "personnel data",
            PayData.DataType.unknown => "unknown type"
        };
        if (data.type == PayData.DataType.pay) {
            dashSpacer.text = ":";
            valueamount.text = $"{data.value}";
            creditIndicator.SetActive(true);
            valueamount.gameObject.SetActive(true);
        } else {
            dashSpacer.text = "";
            valueamount.text = $"";
            creditIndicator.SetActive(false);
            valueamount.gameObject.SetActive(false);
        }
    }

    public void DownloadButtonCallback() {
        PayData payData = node.payData;
        List<CyberNode> downloadPath = GameManager.I.gameData.levelState.delta.cyberGraph.GetPathToNearestDownloadPoint(node);
        NetworkAction networkAction = new NetworkAction() {
            title = $"downloading {payData.filename}...",
            softwareTemplate = SoftwareTemplate.Download(),
            lifetime = 2f,
            toNode = node,
            timerRate = 1f,
            payData = payData,
            path = downloadPath,
        };
        GameManager.I.AddNetworkAction(networkAction);
    }
}
