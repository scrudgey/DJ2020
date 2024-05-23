using System;
using System.Collections;
using System.Collections.Generic;
using Easings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class EffectView : MonoBehaviour {
    [HideInInspector]
    public SoftwareEffect effect;
    public GraphIconReference graphIconReference;
    [Header("display")]
    public Image icon;
    public TextMeshProUGUI description;
    public TextMeshProUGUI title;
    public void Initialize(SoftwareEffect effect) {
        this.effect = effect;
        title.text = effect.TitleString();
        description.text = effect.JustDescription();
        icon.sprite = graphIconReference.SoftwareEffectSprite(effect);
    }

}
