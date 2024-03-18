using System.Collections;
using System.Collections.Generic;
using Easings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BarMontageButton : MonoBehaviour {
    public Image portrait;
    public TextMeshProUGUI title;
    public TextMeshProUGUI description;
    public MontageViewController montageViewController;
    public Tactic tactic;
    public void Initialize(MontageViewController montageViewController, Tactic tactic) {
        this.montageViewController = montageViewController;
        this.tactic = tactic;
        portrait.sprite = tactic.vendorSprite;
        title.text = tactic.vendorName;
        description.text = tactic.vendorDescription;
    }
    public void ClickCallback() {
        montageViewController.BarMontageButtonCallback(this);
    }
}
