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
    public TextMeshProUGUI viewSize;
    [Header("effects")]
    public GameObject effectViewPrefab;
    public Transform effectViewContainer;
    CyberNode target;
    CyberNode origin;
    List<CyberNode> path;
    public void Initialize(CyberNode target, CyberNode origin, List<CyberNode> path) {
        this.target = target;
        this.origin = origin;
        this.path = path;
    }

    public void DisplayState(SoftwareState state) {
        DisplayTemplate(state.template);

        string requirements = "requirements:\n";
        if (target != null) {
            if (state.template.conditions.Count == 0) {
                requirements += "none";
            } else {
                foreach (SoftwareCondition condition in state.template.conditions) {
                    requirements += condition.DescriptionStringWithColor(target, origin, path) + "\n";
                }
            }

        }
        viewRequirements.text = requirements;

        if (state.template.infiniteCharges) {
            viewCharges.text = $"charges: unlimited";
        } else {
            viewCharges.text = $"charges: {state.charges}/{state.template.maxCharges}";
        }
        viewSize.gameObject.SetActive(false);

    }

    public void DisplayTemplate(SoftwareTemplate template) {
        // set view 
        titleIcon.sprite = template.icon;
        viewTitle.text = template.name;

        // description
        string description = "";
        if (template.softwareType == SoftwareTemplate.SoftwareType.exploit) {
            description = "type: targeted\n";
        } else if (template.softwareType == SoftwareTemplate.SoftwareType.virus) {
            description = "type: virus\n";
        }

        // effects
        foreach (Transform child in effectViewContainer) {
            Destroy(child.gameObject);
        }
        foreach (SoftwareEffect effect in template.effects) {
            GameObject obj = GameObject.Instantiate(effectViewPrefab);
            EffectView effectView = obj.GetComponent<EffectView>();
            effectView.Initialize(effect);
            obj.transform.SetParent(effectViewContainer, false);
        }
        viewDescription.text = description;

        // requirements
        string requirements = "requirements:\n";
        if (template.conditions.Count == 0) {
            requirements += "none";
        } else {
            foreach (SoftwareCondition condition in template.conditions) {
                requirements += condition.DescriptionString() + "\n";
            }
        }
        viewRequirements.text = requirements;

        if (template.infiniteCharges) {
            viewCharges.text = $"charges: unlimited";
        } else {
            viewCharges.text = $"charges: {template.maxCharges}";
        }

        viewSize.text = $"size: {template.CalculateSize()} MB";
    }

}
