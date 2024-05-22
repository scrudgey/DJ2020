using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SoftwareButton : MonoBehaviour {
    public Image icon;
    public TextMeshProUGUI caption;
    public TextMeshProUGUI levelCaption;
    SoftwareState state;
    SoftwareTemplate template;
    public void Initialize(SoftwareState state) {
        this.state = state;
        icon.sprite = state.template.icon;
        levelCaption.text = state.template.infiniteCharges ? "-" : $"{state.charges}";
    }
    public void Initialize(SoftwareTemplate template) {
        this.template = template;
        icon.sprite = template.icon;
        levelCaption.text = template.infiniteCharges ? "-" : $"{template.maxCharges}";
    }
}

