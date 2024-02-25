using System.Collections;
using System.Collections.Generic;
using Easings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class HackTerminalController : MonoBehaviour {
    public RectTransform rectTransform;
    // public TextMeshProUGUI terminalContent;
    public TerminalAnimation terminalAnimation;
    public NeoCyberNodeIndicator hackOrigin;
    public NeoCyberNodeIndicator hackTarget;
    public TerminalAnimation terminal;
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
    CyberNodeStatus currentCyberNodeStatus;
    NodeVisibility currentNodeVisibility;
    int currentLockLevel;
    public void ConfigureHackTerminal(NeoCyberNodeIndicator hackOrigin, NeoCyberNodeIndicator hackTarget, List<CyberNode> path) {
        bool changeDetected = this.hackOrigin != hackOrigin || this.hackTarget != hackTarget;
        if (changeDetected) {
            terminalAnimation.Clear();
        }
        this.hackOrigin = hackOrigin;
        this.hackTarget = hackTarget;
        this.path = path;
        modalController.Initialize(this);

        if (hackTarget != null) {
            CyberNodeStatus nodeStatus = hackTarget.node.getStatus();
            changeDetected |= nodeStatus != currentCyberNodeStatus;
            currentCyberNodeStatus = hackTarget.node.getStatus();

            int lockLevel = hackTarget.node.lockLevel;
            changeDetected |= lockLevel != currentLockLevel;
            currentLockLevel = lockLevel;

            NodeVisibility visibility = hackTarget.node.visibility;
            changeDetected |= visibility != currentNodeVisibility;
            currentNodeVisibility = visibility;
        }

        if (hackOrigin == null || hackTarget == null) {
            // terminalContent.text = "";
            terminalAnimation.Clear();
            HideButtons();
        } else {
            attackerTitle.text = hackOrigin.node.nodeTitle;
            attackerIcon.sprite = hackOrigin.iconImage.sprite;
            numberHops.text = $"distance: {path.Count - 1}/{2}";
            ShowButtons();
            if (changeDetected) {
                terminalAnimation.DrawBasicNodePrompt(hackTarget.node, null);
            }
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
    public void Show(CyberNode target) {
        DoShowRoutine(true, target);
        isHidden = false;
    }
    public void Hide() {
        DoShowRoutine(false, null);
        isHidden = true;
    }
    void DoShowRoutine(bool value, CyberNode target) {
        // if (showRectRoutine != null) {
        //     StopCoroutine(showRectRoutine);
        // }
        if (value) {
            terminalAnimation.DrawBasicNodePrompt(target, ShowRect(true));
            // showRectRoutine = StartCoroutine(Toolbox.ChainCoroutines(
            //             ShowRect(value),
            //             terminalAnimation.DrawBasicNodePrompt(target)
            //             ));
        } else {
            showRectRoutine = StartCoroutine(ShowRect(false));
        }
    }

    IEnumerator ShowRect(bool value) {
        float startValue = rectTransform.rect.height;
        float finalValue = value ? 365f : 0f;
        return Toolbox.Ease(null, 0.25f, startValue, finalValue, PennerDoubleAnimation.ExpoEaseOut, (amount) => {
            rectTransform.sizeDelta = new Vector2(505f, amount);
        }, unscaledTime: true);
    }

    public void HackButtonCallback() {
        GameManager.I.ShowMenu(MenuType.softwareModal);
    }
    public void DeploySoftware(SoftwareState state) {
        state.charges -= 1;
        NetworkAction networkAction = state.template.ToNetworkAction(path, hackTarget.node);
        GameManager.I.AddNetworkAction(networkAction);
        terminalAnimation.HandleSoftwareCallback(state);
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
        terminalAnimation.HandleDownload(payData);
        // else if (effect.type == SoftwareEffect.Type.download) {
        //     networkAction.title = $"downloading {node.payData.filename}...";
        //     if (node.isManualHackerTarget) {
        //         networkAction.fromPlayerNode = true;
        //     }
        // } 
    }
    public void UtilityButtonCallback() {
        GameManager.I.SetCyberNodeUtilityState(hackTarget.node, false);
        terminalAnimation.HandleUtility();
    }
}
