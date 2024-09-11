using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Easings;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
public class HackTerminalController : MonoBehaviour {
    public AudioSource audioSource;
    public RectTransform rectTransform;
    public TerminalAnimation terminalAnimation;
    public NeoCyberNodeIndicator hackOrigin;
    public NeoCyberNodeIndicator hackTarget;
    [Header("hack buttons")]
    public Transform buttonContainer;
    public GameObject buttonPrefab;
    [Header("title")]
    public TextMeshProUGUI attackerTitle;
    public Image attackerIcon;
    public TextMeshProUGUI numberHops;
    // [Header("modal")]
    // public SoftwareModalController modalController;
    [Header("sounds")]
    public AudioClip[] showTerminalSounds;
    public AudioClip[] changeInterfaceSounds;
    public List<CyberNode> path;
    [Header("interface")]
    public RectTransform buttonBarRect;
    public RectTransform softwareViewRect;
    public SoftwareView softwareView;
    public GameObject lockBox;
    public CanvasGroup buttonGroup;


    CyberNodeStatus currentCyberNodeStatus;
    NodeVisibility currentNodeVisibility;
    int currentLockLevel;
    Coroutine showRoutine;
    Coroutine showRectRoutine;
    bool isHidden;
    Coroutine interfaceRoutine;

    SoftwareButton activeSelector;

    public void ConfigureHackTerminal(NeoCyberNodeIndicator hackOrigin, NeoCyberNodeIndicator hackTarget, List<CyberNode> path) {
        bool changeDetected = this.hackOrigin != hackOrigin || this.hackTarget != hackTarget;
        this.hackOrigin = hackOrigin;
        this.hackTarget = hackTarget;
        this.path = path;

        UpdateLockBox();

        // modalController.Initialize(this, GameManager.I.gameData.levelState.delta.softwareStates);
        PopulateSoftwareSelectors(GameManager.I.gameData.levelState.delta.softwareStates);

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

        if (hackOrigin != null) {
            attackerTitle.text = hackOrigin.node.nodeTitle;
            attackerIcon.sprite = hackOrigin.iconImage.sprite;
            numberHops.text = $"distance: {path.Count - 1}/{2}";
        }
    }
    public void Show(CyberNode target) {
        DoShowRoutine(true, target);
        isHidden = false;
        Toolbox.RandomizeOneShot(audioSource, showTerminalSounds);
    }
    public void Hide() {
        DoShowRoutine(false, null);
        isHidden = true;
    }
    void DoShowRoutine(bool value, CyberNode target) {
        if (value) {
            showRectRoutine = StartCoroutine(ShowRect(true));
        } else {
            showRectRoutine = StartCoroutine(ShowRect(false));
        }
    }
    void UpdateLockBox() {
        int numberActions = GameManager.I.gameData?.levelState?.delta?.cyberGraph?.networkActions?.Values.SelectMany(x => x).Count() ?? 0;
        lockBox.SetActive(numberActions > 0);
        buttonGroup.interactable = numberActions == 0;
    }

    IEnumerator ShowRect(bool value) {
        float startValue = rectTransform.rect.height;
        float finalValue = value ? 375f : 0f;
        return Toolbox.Ease(null, 0.25f, startValue, finalValue, PennerDoubleAnimation.ExpoEaseOut, (amount) => {
            rectTransform.sizeDelta = new Vector2(350f, amount);
        }, unscaledTime: true);
    }

    public void HackButtonCallback() {
        GameManager.I.ShowMenu(MenuType.softwareModal);
        CutsceneManager.I.HandleTrigger("hack_software_open");
    }
    public void DeploySoftware(SoftwareState state) {
        if (!state.template.infiniteCharges)
            state.charges -= 1;
        NetworkAction networkAction = state.template.ToNetworkAction(path, hackTarget.node);
        GameManager.I.AddNetworkAction(networkAction);
        Toolbox.RandomizeOneShot(audioSource, state.template.deploySounds.ToArray());
        UpdateLockBox();
    }

    void PopulateSoftwareSelectors(List<SoftwareState> softwareStates) {
        foreach (Transform child in buttonContainer) {
            Destroy(child.gameObject);
        }
        CyberNode target = hackTarget?.node ?? null;
        CyberNode origin = hackOrigin?.node ?? null;
        softwareView.Initialize(target, origin, path);

        // spawn buttons
        foreach (SoftwareState state in softwareStates) {
            CreateSoftwareSelector(state, target, origin);
        }
    }
    SoftwareButton CreateSoftwareSelector(SoftwareState softwareState, CyberNode target, CyberNode origin) {
        GameObject obj = GameObject.Instantiate(buttonPrefab);
        SoftwareButton selector = obj.GetComponent<SoftwareButton>();
        bool softwareEnabled = false;
        if (target != null) {
            softwareEnabled = softwareState.EvaluateCondition(target, origin, path) && softwareState.charges > 0;
        }
        selector.Initialize(softwareState, SoftwareButtonClicked, softwareEnabled);
        selector.transform.SetParent(buttonContainer, false);
        // selectorIndicators.Add(softwareState.template.name, obj.GetComponent<RectTransform>());
        return selector;
    }

    public void SoftwareButtonClicked(SoftwareButton button) {
        CyberNode target = hackTarget?.node ?? null;
        CyberNode origin = hackOrigin?.node ?? null;
        softwareView.Initialize(target, origin, path);
        softwareView.DisplayState(button.state);
        activeSelector = button;
        ChangeInterfacePanel(showHackButtons: false);
    }
    public void SoftwareDeployCallback() {
        // GameManager.I.CloseMenu();
        ChangeInterfacePanel(showHackButtons: true);
        if (activeSelector != null) {
            DeploySoftware(activeSelector.state);
            CutsceneManager.I.HandleTrigger($"software_deploy_{activeSelector.state.template.name}");
        }
    }
    public void BackButtonCallback() {
        ChangeInterfacePanel(showHackButtons: true);
    }

    void ChangeInterfacePanel(bool showHackButtons = true) {
        if (interfaceRoutine != null) {
            StopCoroutine(interfaceRoutine);
        }
        interfaceRoutine = StartCoroutine(MoveInterfaces(showHackButtons: showHackButtons));
        Toolbox.RandomizeOneShot(audioSource, changeInterfaceSounds);
    }
    IEnumerator MoveInterfaces(bool showHackButtons = true) {
        float initialAmount = buttonBarRect.anchoredPosition.x;
        float finalAmount = showHackButtons ? 0f : -350f;
        yield return Toolbox.Ease(null, 0.2f, initialAmount, finalAmount, PennerDoubleAnimation.ExpoEaseOut, (amount) => {
            buttonBarRect.anchoredPosition = new Vector2(amount, 0f);
            softwareViewRect.anchoredPosition = new Vector2(amount + 350f, 0f);
        });
    }
}
