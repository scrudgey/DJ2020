using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

namespace UI {

    public class WeaponUIHandler : MonoBehaviour {
        private GunHandler target;
        public TextMeshProUGUI ammoIndicator;
        public TextMeshProUGUI ammoImageCaption;
        public Image ammoImage;

        public void Bind(GameObject newTargetObject) {
            // Debug.Log($"ammo bind: {newTargetObject}");
            if (target != null) {
                target.OnValueChanged -= HandleValueChanged;
            }
            target = newTargetObject.GetComponentInChildren<GunHandler>();
            if (target != null) {
                target.OnValueChanged += HandleValueChanged;
                HandleValueChanged(target);
            }
        }

        public void HandleValueChanged(GunHandler gun) {
            if (gun != null && gun.gunInstance != null) {
                ammoIndicator.text = $"A: {gun.gunInstance.TotalAmmo()}/{gun.gunInstance.MaxAmmo()}";
                ammoImageCaption.text = gun.gunInstance.baseGun.name;
                ammoImage.sprite = gun.gunInstance.baseGun.image;
            } else {
                ammoIndicator.text = $"A: -/-";
                ammoImageCaption.text = "";
                ammoImage.sprite = null;
                return;
            }
        }

    }
}
