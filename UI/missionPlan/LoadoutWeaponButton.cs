using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class LoadoutWeaponButton : MonoBehaviour {
    public int weaponIndex;
    public Image weaponImage;
    public TextMeshProUGUI nameText;
    public WeaponState gunState;
    public Image line;
    public CharacterView characterView;
    public void OnClick() {
        characterView.WeaponButtonClicked(this);
    }

    public void WeaponClearCallback() {
        this.gunState = null;
        characterView.WeaponButtonCleared(this);
    }

    public void ApplyGunTemplate(WeaponState gunState) {
        this.gunState = gunState;
        if (gunState == null) {
            nameText.text = "";
            weaponImage.enabled = false;
            line.enabled = false;
        } else {
            line.enabled = true;
            nameText.text = gunState.GetShortName();
            weaponImage.sprite = gunState.GetSprite();
            weaponImage.enabled = true;
        }
    }
}
