using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class PerkButton : MonoBehaviour {
    public Image icon;
    public GameObject lockIcon;
    public TextMeshProUGUI caption;
    public Image[] pips;
    public Perk perk;
    public Sprite pipActive;
    public Sprite pipDisabled;

    PlayerState state;
    PerkMenuController controller;

    public void Initialize(PerkMenuController controller, PlayerState state) {
        this.controller = controller;
        this.state = state;

        icon.sprite = perk.icon;
        caption.text = perk.readableName;
        if (perk.IsMultiStagePerk()) {

        } else {
            foreach (Image pip in pips) {
                pip.gameObject.SetActive(false);
            }
        }


        SetActiveStatus();
    }

    public void SetActiveStatus() {
        if (perk.IsMultiStagePerk()) {
            for (int i = 1; i < perk.stages + 1; i++) {
                if (state.PerkLevelIsActivated(perk, i)) {
                    pips[i - 1].sprite = pipActive;
                } else {
                    pips[i - 1].sprite = pipDisabled;
                }
            }
        }
        if (state.PerkIsFullyActivated(perk)) {

        }

        if (perk.CanBePurchased(state)) {
            lockIcon.SetActive(false);
        } else {
            lockIcon.SetActive(true);
        }
    }

    public void ClickCallback() {
        controller.PerkButtonCallback(this);
    }


#if UNITY_EDITOR
    private void OnDrawGizmos() {
        Handles.Label(transform.position, $"{perk.readableName}");
    }
#endif
}
