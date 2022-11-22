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
        PrefabPool ammoSpentPool;
        PrefabPool ammoPipPool;
        void Awake() {
            ammoSpentPool = PoolManager.I.RegisterPool(ammoSpentPrefab, 20);
            ammoPipPool = PoolManager.I.RegisterPool(ammoPipPrefab, 50);
            liveAmmoPipsTop = new List<AmmoPip>();
            liveAmmoPipsBottom = new List<AmmoPip>();
            foreach (Transform child in ammoPipContainer) {
                Destroy(child.gameObject);
            }
            foreach (Transform child in lowerAmmoPipContainer) {
                Destroy(child.gameObject);
            }
            foreach (Transform child in chamberPipContainer) {
                Destroy(child.gameObject);
            }
        }
        void ClearAllPips() {
            liveAmmoPipsTop = new List<AmmoPip>();
            liveAmmoPipsBottom = new List<AmmoPip>();
            List<GameObject> recallMe = new List<GameObject>();
            if (ammoPipContainer != null) {
                foreach (Transform child in ammoPipContainer) {
                    recallMe.Add(child.gameObject);
                }
            }
            if (lowerAmmoPipContainer != null)
                foreach (Transform child in lowerAmmoPipContainer) {
                    recallMe.Add(child.gameObject);
                }
            if (chamberPipContainer != null) {
                foreach (Transform child in chamberPipContainer) {
                    recallMe.Add(child.gameObject);

                }
            }
            foreach (GameObject obj in recallMe) {
                ammoPipPool.RecallObject(obj);
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
            if (!lowerAmmoRowObject.activeInHierarchy) {
                foreach (Transform child in lowerAmmoPipContainer) {
                    ammoPipPool.RecallObject(child.gameObject);
                }
            }
        }

        void RectifyPips(int clip, int chamber, bool createSpentPip) {
            while (chamberPipContainer.Cast<Transform>().Count(child => child.gameObject.activeInHierarchy) > chamber) {
                Transform child = chamberPipContainer.GetChild(0);
                ammoPipPool.RecallObject(child.gameObject);
                if (createSpentPip)
                    CreateSpentPip();
            }
            while (chamberPipContainer.Cast<Transform>().Count(child => child.gameObject.activeInHierarchy) < chamber) {
                GameObject newPip = ammoPipPool.GetObject(chamberPipContainer.position);
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
            GameObject spentPip = ammoSpentPool.GetObject(chamberPipContainer.position);
            AmmoSpent spent = spentPip.GetComponent<AmmoSpent>();
            spent.pool = ammoSpentPool;
            spent.SetSprite(target.gunInstance.template.type);
            spentPip.transform.SetParent(transform, true);
        }
        void RemoveBulletPip(bool createSpentPip) {
            if (totalAmmoPips() > 15) {
                AmmoPip pip1 = liveAmmoPipsTop[0];
                liveAmmoPipsTop.Remove(pip1);
                pip1.Disappear(ammoPipPool);

                // if (liveAmmoPipsBottom.Count > 0) {
                AmmoPip pip2 = liveAmmoPipsBottom[0];
                liveAmmoPipsBottom.Remove(pip2);
                pip2.Disappear(ammoPipPool);
                // }

                GameObject newPip = ammoPipPool.GetObject(ammoPipContainer.transform.position);
                AmmoPip pip = newPip.GetComponent<AmmoPip>();
                pip.SetSprite(target.gunInstance.template.type);
                newPip.transform.SetParent(ammoPipContainer, false);
                pip.layoutRect = ammoPipRect;
                liveAmmoPipsTop.Add(pip);
            } else {
                AmmoPip pip = liveAmmoPipsTop[0];
                liveAmmoPipsTop.Remove(pip);
                pip.Disappear(ammoPipPool);
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

            GameObject newPip = ammoPipPool.GetObject(container.transform.position);
            AmmoPip pip = newPip.GetComponent<AmmoPip>();
            pip.SetSprite(target.gunInstance.template.type);
            newPip.transform.SetParent(container, false);
            pip.layoutRect = containerRect;
            ammoList.Add(pip);
        }
    }
}
