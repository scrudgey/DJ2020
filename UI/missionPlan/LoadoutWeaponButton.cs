using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class LoadoutWeaponButton : MonoBehaviour {
    public int weaponIndex;
    public MissionPlanLoadoutController loadoutController;
    public Image weaponImage;
    public TextMeshProUGUI nameText;
    public GunState gunState;
    public Image line;
    public void OnClick() {
        loadoutController.WeaponSlotClicked(weaponIndex, gunState);
    }

    public void WeaponClearCallback() {
        this.gunState = null;
        nameText.text = "";
        weaponImage.enabled = false;
        loadoutController.WeaponSlotClicked(weaponIndex, gunState, clear: true);
        line.enabled = false;
    }

    public void ApplyGunTemplate(GunState gunState) {
        this.gunState = gunState;
        line.enabled = gunState != null;
        if (gunState == null)
            return;
        nameText.text = gunState.getShortName();
        weaponImage.sprite = gunState.GetSprite();
        weaponImage.enabled = true;
    }
}
