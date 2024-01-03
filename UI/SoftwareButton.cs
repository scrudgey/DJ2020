using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class SoftwareButton : MonoBehaviour {
    public bool initializeOnStart;
    public CyberdeckUIController cyberdeckController;
    public Button button;
    public SoftwareEffect effect;
    [Header("UI")]
    public Sprite scanIcon;
    public Sprite downloadIcon;
    public Sprite unlockIcon;
    public Sprite compromiseIcon;
    public Image icon;
    public TextMeshProUGUI caption;
    public TextMeshProUGUI levelCaption;
    void Start() {
        if (initializeOnStart) {
            Initialize(cyberdeckController);
        }
    }
    public void Initialize(CyberdeckUIController cyberdeckController) {
        this.cyberdeckController = cyberdeckController;
        switch (effect.type) {
            case SoftwareEffect.Type.scan:
                icon.sprite = scanIcon;
                break;
            case SoftwareEffect.Type.download:
                icon.sprite = downloadIcon;
                break;
            case SoftwareEffect.Type.unlock:
                icon.sprite = unlockIcon;
                break;
            case SoftwareEffect.Type.compromise:
                icon.sprite = compromiseIcon;
                break;
        }
        caption.text = effect.name;
        levelCaption.text = effect.level.ToString();
    }
    public void OnClick() {
        cyberdeckController.SoftwareButtonCallback(this);
    }
}

[System.Serializable]
public class SoftwareEffect {
    public enum Type { scan, download, unlock, compromise, none }
    public Type type;
    public int level;
    public string name;
    public void ApplyToNode(CyberNode node) {
        switch (type) {
            case Type.scan:
                node.visibility = NodeVisibility.mapped;
                break;
            case Type.download:
                if (node.type == CyberNodeType.datanode) {
                    Debug.Log($"download " + node.payData);
                    GameManager.I.AddPayDatas(node.payData);
                    node.dataStolen = true;
                }
                break;
            case Type.unlock:
                node.lockLevel = 0;
                break;
            case Type.compromise:
                node.compromised = true;
                break;
        }
    }
}