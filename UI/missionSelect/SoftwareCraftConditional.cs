using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Easings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class SoftwareCraftConditional : MonoBehaviour {
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI costText;

    public void Initialize(SoftwareCondition condition) {
        descriptionText.text = condition.DescriptionString();
        costText.text = $"{condition.Cost()}";
    }
}
