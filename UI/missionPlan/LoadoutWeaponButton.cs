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
    public GunTemplate gunTemplate;
    public void OnClick() {
        loadoutController.WeaponSlotClicked(weaponIndex, gunTemplate);
    }

    public void WeaponClearCallback() {
        this.gunTemplate = null;
        nameText.text = "";
        weaponImage.enabled = false;
        loadoutController.WeaponSlotClicked(weaponIndex, gunTemplate, clear: true);
    }

    public void ApplyGunTemplate(GunTemplate template) {
        this.gunTemplate = template;
        if (template == null)
            return;
        nameText.text = template.name;
        weaponImage.sprite = template.image;
        weaponImage.enabled = true;
    }
}
