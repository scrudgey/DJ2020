using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI {

    public class WeaponUIHandler : IBinder<GunHandler> {
        public TextMeshProUGUI gunTitle;
        public TextMeshProUGUI ammoCounter;
        public Image gunImage;

        public Transform ammoPipContainer;
        public RectTransform ammoPipRect;

        public Transform lowerAmmoPipContainer;
        public RectTransform lowerAmmoPipRect;
        public GameObject lowerAmmoRowObject;

        public Transform chamberPipContainer;
        public GameObject ammoPipPrefab;
        public GameObject ammoSpentPrefab;
        List<AmmoPip> liveAmmoPipsTop = new List<AmmoPip>();
        List<AmmoPip> liveAmmoPipsBottom = new List<AmmoPip>();
        int clipsize;
        void Start() {
            ClearAllPips();
        }
        void ClearAllPips() {
            liveAmmoPipsTop = new List<AmmoPip>();
            liveAmmoPipsBottom = new List<AmmoPip>();
            if (ammoPipContainer != null) {
                foreach (Transform child in ammoPipContainer) {
                    Destroy(child.gameObject);
                }
            }
            if (lowerAmmoPipContainer != null)
                foreach (Transform child in lowerAmmoPipContainer) {
                    Destroy(child.gameObject);
                }
            if (chamberPipContainer != null) {
                foreach (Transform child in chamberPipContainer) {
                    Destroy(child.gameObject);
                }
            }
        }
        override public void HandleValueChanged(GunHandler gun) {
            if (gun.isSwitchingWeapon) {
                ClearAllPips();
            }
            if (gun.HasGun()) {
                clipsize = gun.gunInstance.template.clipSize;
                gunImage.enabled = true;
                gunImage.sprite = gun.gunInstance.template.image;
                gunTitle.text = gun.gunInstance.template.name;
                string totalAmmoText = gun.gunInstance.template.clipSize > 9 ? gun.gunInstance.TotalAmmo().ToString("D2") : gun.gunInstance.TotalAmmo().ToString();
                ammoCounter.text = $"{totalAmmoText} / {gun.gunInstance.MaxAmmo()}";
                bool createSpentPip = gun.isShooting;
                RectifyPips(gun.gunInstance.delta.clip, gun.gunInstance.delta.chamber, createSpentPip);
            } else {
                clipsize = 0;
                gunImage.enabled = false;
                gunImage.sprite = null;
                ammoCounter.text = $"-/-";
                gunTitle.text = "";
                RectifyPips(0, 0, false);
            }

            lowerAmmoRowObject.SetActive(clipsize > 15);
            if (!lowerAmmoRowObject.activeInHierarchy && lowerAmmoPipContainer.childCount > 0) {
                foreach (Transform child in lowerAmmoPipContainer) {
                    Destroy(child.gameObject);
                }
            }
        }

        void RectifyPips(int clip, int chamber, bool createSpentPip) {
            while (chamberPipContainer.childCount > chamber && chamberPipContainer.childCount > 0) {
                Transform child = chamberPipContainer.GetChild(0);
                Destroy(child.gameObject);
                child.SetParent(null, true);

                if (createSpentPip)
                    CreateSpentPip();
            }
            while (chamberPipContainer.childCount < chamber) {
                GameObject newPip = GameObject.Instantiate(ammoPipPrefab);
                AmmoPip pip = newPip.GetComponent<AmmoPip>();
                pip.SetSprite(target.gunInstance.template.type);
                newPip.transform.SetParent(chamberPipContainer, false);
                newPip.transform.SetAsFirstSibling();
            }

            while (totalAmmoPips() > clip) {
                RemoveBulletPip(createSpentPip);
            }
            while (totalAmmoPips() < clip) {
                AddBulletPip();
            }
        }

        int totalAmmoPips() => liveAmmoPipsTop.Count + liveAmmoPipsBottom.Count;
        void CreateSpentPip() {
            GameObject spentPip = GameObject.Instantiate(ammoSpentPrefab, chamberPipContainer.position, Quaternion.identity);
            AmmoSpent spent = spentPip.GetComponent<AmmoSpent>();
            spent.SetSprite(target.gunInstance.template.type);
            spentPip.transform.SetParent(transform, true);
        }
        void RemoveBulletPip(bool createSpentPip) {
            if (totalAmmoPips() > 15) {
                AmmoPip pip1 = liveAmmoPipsTop[0];
                liveAmmoPipsTop.Remove(pip1);
                pip1.Disappear();

                // if (liveAmmoPipsBottom.Count > 0) {
                AmmoPip pip2 = liveAmmoPipsBottom[0];
                liveAmmoPipsBottom.Remove(pip2);
                pip2.Disappear();
                // }

                GameObject newPip = GameObject.Instantiate(ammoPipPrefab);
                AmmoPip pip = newPip.GetComponent<AmmoPip>();
                pip.SetSprite(target.gunInstance.template.type);
                newPip.transform.SetParent(ammoPipContainer, false);
                pip.layoutRect = ammoPipRect;
                liveAmmoPipsTop.Add(pip);
            } else {
                AmmoPip pip = liveAmmoPipsTop[0];
                liveAmmoPipsTop.Remove(pip);
                pip.Disappear();
            }
            if (createSpentPip)
                CreateSpentPip();
        }
        void AddBulletPip() {
            Transform container = null;
            RectTransform containerRect = null;
            List<AmmoPip> ammoList = null;

            if (totalAmmoPips() > 15) {
                container = lowerAmmoPipContainer;
                containerRect = lowerAmmoPipRect;
                ammoList = liveAmmoPipsBottom;
            } else {
                container = ammoPipContainer;
                containerRect = ammoPipRect;
                ammoList = liveAmmoPipsTop;
            }

            GameObject newPip = GameObject.Instantiate(ammoPipPrefab);
            AmmoPip pip = newPip.GetComponent<AmmoPip>();
            pip.SetSprite(target.gunInstance.template.type);

            newPip.transform.SetParent(container, false);
            pip.layoutRect = containerRect;

            ammoList.Add(pip);
        }
    }
}
