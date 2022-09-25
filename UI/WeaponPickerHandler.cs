using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
public class WeaponPickerHandler : MonoBehaviour {
    public VRMissionDesigner missionDesigner;
    public GunTemplate gun;
    public TextMeshProUGUI nameText;
    public void OnClick() {
        missionDesigner.WeaponPickerCallback(this);
    }
    public void Configure(GunTemplate gun, VRMissionDesigner missionDesigner) {
        this.gun = gun;
        this.missionDesigner = missionDesigner;
        nameText.text = gun.name;
    }
}
