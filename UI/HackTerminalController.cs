using System.Collections;
using System.Collections.Generic;
using Easings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class HackTerminalController : MonoBehaviour {
    public RectTransform rectTransform;
    public TextMeshProUGUI terminalContent;
    public NeoCyberNodeIndicator hackOrigin;
    public NeoCyberNodeIndicator hackTarget;
    [Header("title")]
    public TextMeshProUGUI attackerTitle;
    public Image attackerIcon;
    public TextMeshProUGUI numberHops;
    [Header("modal")]
    public SoftwareModalController modalController;


    [Header("buttons")]
    public GameObject downloadButton;
    public GameObject passwordButton;
    public GameObject utilityButton;
    public TextMeshProUGUI utilityButtonText;

    Coroutine showRectRoutine;
    bool isHidden;
    List<CyberNode> path;
    public void ConfigureHackTerminal(NeoCyberNodeIndicator hackOrigin, NeoCyberNodeIndicator hackTarget, List<CyberNode> path) {
        this.hackOrigin = hackOrigin;
        this.hackTarget = hackTarget;
        this.path = path;
        modalController.Initialize(this);

        // if (hackOrigin == null || hackOrigin == hackTarget) {
        //     Hide();
        // } else {
        //     Show();
        // }
        if (hackOrigin == null || hackTarget == null) {
            terminalContent.text = "";
            HideButtons();
        } else {
            attackerTitle.text = hackOrigin.node.nodeTitle;
            attackerIcon.sprite = hackOrigin.iconImage.sprite;
            numberHops.text = $"distance: {path.Count - 1}/{2}";
            if (hackTarget.node.visibility == NodeVisibility.unknown || hackTarget.node.visibility == NodeVisibility.mystery) {
                terminalContent.text = "> ping 127.0.5.10\nPING 127.0.5.10: 56 data bytes\n64 bytes from 127.0.5.10: icmp_seq=0 ttl=64 time=0.118 ms";
            } else {
                // target is visible / mapped
                if (hackTarget.node.getStatus() == CyberNodeStatus.compromised) {
                    terminalContent.text = "root @ 127.0.05.10 > command? █";
                } else {
                    if (hackTarget.node.lockLevel > 0) {
                        terminalContent.text = "> ssh 127.0.5.10\n* welcome to city sense/net! *\nnew users must register with sysadmin.\n\n    enter password:█";
                    } else {
                        terminalContent.text = "> ssh 127.0.5.10\n* welcome to city sense/net! *\nnew users must register with sysadmin.\n\n    enter password:*****\nACCESS GRANTED\n\nbash>█";
                    }
                }
            }
            ShowButtons();
        }
    }
    void ShowButtons() {
        passwordButton.SetActive(hackTarget.node.visibility > NodeVisibility.mystery && hackTarget.node.lockLevel > 0);
        downloadButton.SetActive(hackTarget.node.visibility > NodeVisibility.mystery && hackTarget.node.lockLevel == 0 && hackTarget.node.payData != null && !hackTarget.node.dataStolen && hackTarget.node.type == CyberNodeType.datanode);
        utilityButton.SetActive(hackTarget.node.visibility > NodeVisibility.mystery && hackTarget.node.lockLevel == 0 && hackTarget.node.type == CyberNodeType.utility);
    }
    void HideButtons() {
        passwordButton.SetActive(false);
        downloadButton.SetActive(false);
        utilityButton.SetActive(false);
    }
    public void Show() {
        // if (!isHidden) return;
        Debug.Log("show terminal");
        DoShowRoutine(true);
        isHidden = false;
    }
    public void Hide() {
        // if (isHidden) return;
        Debug.Log("hiding terminal");
        DoShowRoutine(false);
        isHidden = true;
    }
    void DoShowRoutine(bool value) {
        if (showRectRoutine != null) {
            StopCoroutine(showRectRoutine);
        }
        showRectRoutine = StartCoroutine(ShowRect(value));
    }

    IEnumerator ShowRect(bool value) {
        float startValue = rectTransform.rect.height;
        float finalValue = value ? 440f : 0f;
        return Toolbox.Ease(null, 0.25f, startValue, finalValue, PennerDoubleAnimation.ExpoEaseOut, (amount) => {
            rectTransform.sizeDelta = new Vector2(505f, amount);
        }, unscaledTime: true);
    }

    public void HackButtonCallback() {
        GameManager.I.ShowMenu(MenuType.softwareModal);
    }
    public void DeploySoftware(SoftwareState state) {
        Debug.Log($"deploying software {state.template.name}");
        NetworkAction networkAction = state.template.ToNetworkAction(path, hackTarget.node);
        GameManager.I.AddNetworkAction(networkAction);
    }


    public void PasswordButtonCallback() {
        Debug.Log("password");
    }
    public void DownloadButtonCallback() {
        PayData payData = hackTarget.node.payData;
        List<CyberNode> downloadPath = GameManager.I.gameData.levelState.delta.cyberGraph.GetPathToNearestDownloadPoint(hackTarget.node);
        NetworkAction networkAction = new NetworkAction() {
            title = $"downloading {payData.filename}...",
            softwareTemplate = SoftwareTemplate.Download(),
            lifetime = 2f,
            toNode = path[path.Count - 1],
            timerRate = 1f,
            payData = payData,
            path = downloadPath,
            fromPlayerNode = hackTarget.node.isManualHackerTarget
        };
        GameManager.I.AddNetworkAction(networkAction);
        // else if (effect.type == SoftwareEffect.Type.download) {
        //     networkAction.title = $"downloading {node.payData.filename}...";
        //     if (node.isManualHackerTarget) {
        //         networkAction.fromPlayerNode = true;
        //     }
        // } 
    }
    public void UtilityButtonCallback() {
        GameManager.I.SetCyberNodeUtilityState(hackTarget.node, false);
    }
}
