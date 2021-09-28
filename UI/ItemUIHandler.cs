using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UI {
    public class ItemUIHandler : MonoBehaviour {
        private ItemHandler target;
        public Image itemImage;
        public TextMeshProUGUI itemTitle;
        public TextMeshProUGUI itemCaption;
        public void Bind(GameObject newTargetObject) {
            if (target != null) {
                target.OnValueChanged -= HandleValueChanged;
            }
            target = newTargetObject.GetComponentInChildren<ItemHandler>();
            if (target != null) {
                target.OnValueChanged += HandleValueChanged;
                HandleValueChanged(target);
            }
        }

        public void HandleValueChanged(ItemHandler itemHandler) {
            if (itemHandler.activeItem == null) {
                itemImage.sprite = null;
                itemTitle.text = "N/A";
                itemCaption.text = "";
            } else {
                // TODO: fix this up

                itemImage.sprite = itemHandler.activeItem.data.image;
                itemTitle.text = itemHandler.activeItem.data.name;
                // itemCaption.text = itemHandler.activeItem.shortName;
                itemCaption.text = "1/1";
            }
        }
    }
}