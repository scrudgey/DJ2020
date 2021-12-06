using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI {

    public class WeaponUIHandler : MonoBehaviour, IBinder<GunHandler> {
        public GunHandler target { get; set; }
        public TextMeshProUGUI ammoIndicator;
        public TextMeshProUGUI ammoImageCaption;
        public Image ammoImage;

        public void HandleValueChanged(GunHandler gun) {
            if (target.HasGun()) {
                ammoImage.enabled = true;
                ammoIndicator.text = gun.gunInstance.baseGun.name;
                ammoImageCaption.text = $"{gun.gunInstance.TotalAmmo()}/{gun.gunInstance.MaxAmmo()}";
                ammoImage.sprite = gun.gunInstance.baseGun.image;
            } else {
                ammoImage.enabled = false;
                ammoIndicator.text = $"A: -/-";
                ammoImageCaption.text = "";
                ammoImage.sprite = null;
                return;
            }
        }

    }
}
