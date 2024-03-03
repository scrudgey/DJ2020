using System;
using System.Collections;
using System.Collections.Generic;
using Easings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class SoftwareView : MonoBehaviour {
    [Header("view")]
    public Image titleIcon;
    public TextMeshProUGUI viewTitle;
    public TextMeshProUGUI viewDescription;
    public TextMeshProUGUI viewRequirements;
    public TextMeshProUGUI viewCharges;
    PlayerState playerState;
    CyberNode target;
    public void Initialize(PlayerState playerState, CyberNode target) {
        this.playerState = playerState;
        this.target = target;
    }

    // TODO: display template mode
    public void Display(SoftwareState state) {
        Display(state.template);

        viewCharges.text = $"charges: {state.charges}/{state.template.maxCharges}";
    }

    public void Display(SoftwareTemplate template) {
        // set view 
        titleIcon.sprite = template.icon;
        viewTitle.text = template.name;

        string description = "";
        foreach (SoftwareEffect effect in template.effects) {
            description += effect.DescriptionString() + "\n";
        }
        viewDescription.text = description;

        string requirements = "requirements:\n";
        if (target != null) {
            if (template.conditions.Count == 0) {
                requirements += "none";
            } else {
                foreach (SoftwareCondition condition in template.conditions) {
                    requirements += condition.DescriptionString(target) + "\n";
                }
            }

        }
        viewRequirements.text = requirements;

        viewCharges.text = $"charges: {template.maxCharges}";
    }

}
