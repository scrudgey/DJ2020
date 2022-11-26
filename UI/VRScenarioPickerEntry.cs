using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
public class VRScenarioPickerEntry : MonoBehaviour {
    public VRScenarioPicker scenarioPicker;
    public VRMissionTemplate template;
    public TextMeshProUGUI text;
    public void Initialize(VRScenarioPicker scenarioPicker, VRMissionTemplate template) {
        this.scenarioPicker = scenarioPicker;
        this.template = template;
        text.text = template.filename;
    }
    public void Clicked() {
        scenarioPicker.ScenarioPickerEntryCallback(this);
    }
}
