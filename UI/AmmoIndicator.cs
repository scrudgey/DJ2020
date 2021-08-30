using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

namespace UI {

    public class AmmoIndicator : MonoBehaviour {
        private GunHandler target;
        public TextMeshProUGUI ammoIndicator;

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
                ammoIndicator.text = $"Ammo: {gun.gunInstance.TotalAmmo()}";
            } else {
                ammoIndicator.text = $"Ammo: -";
                return;
            }
        }

    }
}
