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
    public Image line;
    public void OnClick() {
        loadoutController.WeaponSlotClicked(weaponIndex, gunTemplate);
    }

    public void WeaponClearCallback() {
        this.gunTemplate = null;
        nameText.text = "";
        weaponImage.enabled = false;
        loadoutController.WeaponSlotClicked(weaponIndex, gunTemplate, clear: true);
        line.enabled = false;
    }

    public void ApplyGunTemplate(GunTemplate template) {
        this.gunTemplate = template;
        line.enabled = template != null;
        if (template == null)
            return;
        nameText.text = template.name;
        weaponImage.sprite = template.image;
        weaponImage.enabled = true;
    }
}
