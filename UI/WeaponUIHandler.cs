using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI {

    public class WeaponUIHandler : IBinder<GunHandler> {
        // public GunHandler target { get; set; }
        public TextMeshProUGUI gunTitle;
        public TextMeshProUGUI ammoCounter;
        public Image gunImage;

        override public void HandleValueChanged(GunHandler gun) {
            if (gun.HasGun()) {
                gunImage.enabled = true;
                gunImage.sprite = gun.gunInstance.template.image;
                gunTitle.text = gun.gunInstance.template.name;
                ammoCounter.text = $"{gun.gunInstance.TotalAmmo()} / {gun.gunInstance.MaxAmmo()}";
            } else {
                gunImage.enabled = false;
                gunImage.sprite = null;
                ammoCounter.text = $"-/-";
                gunTitle.text = "";
                return;
            }
        }

    }
}
