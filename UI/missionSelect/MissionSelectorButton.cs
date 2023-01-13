using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
public class MissionSelectorButton : MonoBehaviour {
    MissionComputerController controller;
    public TextMeshProUGUI text;
    public LevelTemplate template;
    public void Initialize(MissionComputerController controller, LevelTemplate template) {
        this.controller = controller;
        this.template = template;
        text.text = template.levelName;
    }
    public void OnClick() {
        controller.MissionButtonCallback(this);
    }
}
