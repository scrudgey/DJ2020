using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class MissionSelectorButton : MonoBehaviour {
    MissionComputerController controller;
    public TextMeshProUGUI missionName;
    public TextMeshProUGUI faction;
    public TextMeshProUGUI reward;
    public LevelTemplate template;
    public Button button;
    public void Initialize(MissionComputerController controller, LevelTemplate template) {
        this.controller = controller;
        this.template = template;
        missionName.text = template.readableMissionName;
        faction.text = template.faction.name;
        reward.text = template.creditReward.ToString("#,#");
    }
    public void OnClick() {
        controller.MissionButtonCallback(this);
    }
    public void OnMouseOver() {
        controller.MissionButtonMouseover(this);
    }
    public void SetFocus(bool value) {
        if (value) {
            missionName.color = Color.black;
            faction.color = Color.black;
            reward.color = Color.black;
        } else {
            missionName.color = Color.white;
            faction.color = Color.white;
            reward.color = Color.white;
        }
    }
}
