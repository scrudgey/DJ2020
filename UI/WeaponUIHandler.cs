using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Easings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI {

    public class WeaponUIHandler : IBinder<GunHandler> {
        public TextMeshProUGUI gunTitle;
        public TextMeshProUGUI ammoCounter;
        public Image gunImage;
        public RectTransform gunPanelRect;

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
        bool displayIsOpen;
        Coroutine openDisplayCoroutine;
        public void Initialize() {
            displayIsOpen = true;
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
                clipsize = gun.gunInstance.getClipSize();
                gunImage.enabled = true;
                gunImage.sprite = gun.gunInstance.GetSprite();
                gunTitle.text = gun.gunInstance.getShortName();
                string totalAmmoText = clipsize > 9 ? gun.gunInstance.TotalAmmo().ToString("D2") : gun.gunInstance.TotalAmmo().ToString();
                ammoCounter.text = $"{totalAmmoText} / {gun.gunInstance.MaxAmmo()}";
                bool createSpentPip = gun.isShooting;
                RectifyPips(gun.gunInstance.delta.clip, gun.gunInstance.delta.chamber, createSpentPip);
                if (!displayIsOpen) {
                    if (openDisplayCoroutine != null)
                        StopCoroutine(openDisplayCoroutine);
                    openDisplayCoroutine = StartCoroutine(openDisplay());
                }
            } else {
                clipsize = 0;
                gunImage.enabled = false;
                gunImage.sprite = null;
                ammoCounter.text = $"-/-";
                gunTitle.text = "";
                RectifyPips(0, 0, false);
                if (displayIsOpen) {
                    if (openDisplayCoroutine != null)
                        StopCoroutine(openDisplayCoroutine);
                    openDisplayCoroutine = StartCoroutine(closeDisplay());
                }
            }

            lowerAmmoRowObject.SetActive(clipsize > 15);
            if (!lowerAmmoRowObject.activeInHierarchy) {
                foreach (Transform child in lowerAmmoPipContainer) {
                    ammoPipPool.RecallObject(child.gameObject);
                }
            }
        }

        IEnumerator openDisplay() {
            // displayIsOpen = false;
            displayIsOpen = true;

            float timer = 0f;
            float duration = 0.25f;
            while (timer < duration) {
                timer += Time.unscaledDeltaTime;
                float y = (float)PennerDoubleAnimation.Linear(timer, 0f, 1f, duration);
                gunPanelRect.localScale = new Vector3(1f, y, 1f);
                yield return null;
            }
            gunPanelRect.localScale = Vector3.one;
            openDisplayCoroutine = null;
        }
        IEnumerator closeDisplay() {
            // displayIsOpen = false;
            displayIsOpen = false;

            float timer = 0f;
            float duration = 0.25f;
            while (timer < duration) {
                timer += Time.unscaledDeltaTime;
                float y = (float)PennerDoubleAnimation.Linear(timer, 1f, -1f, duration);
                gunPanelRect.localScale = new Vector3(1f, y, 1f);
                yield return null;
            }
            gunPanelRect.localScale = new Vector3(1f, 0, 1f);
            openDisplayCoroutine = null;
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
            AmmoPip topPip = liveAmmoPipsTop[0];
            liveAmmoPipsTop.Remove(topPip);
            topPip.Disappear(ammoPipPool);
            if (totalAmmoPips() > 15) {
                AmmoPip pip2 = liveAmmoPipsBottom[0];
                liveAmmoPipsBottom.Remove(pip2);
                pip2.Disappear(ammoPipPool);

                GameObject newPip = ammoPipPool.GetObject(ammoPipContainer.transform.position);
                AmmoPip pip1 = newPip.GetComponent<AmmoPip>();
                pip1.SetSprite(target.gunInstance.template.type);
                newPip.transform.SetParent(ammoPipContainer, false);
                pip1.layoutRect = ammoPipRect;
                liveAmmoPipsTop.Add(pip1);
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
