using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI {
    public class ItemUIHandler : IBinder<ItemHandler> {
        // public ItemHandler target { get; set; }
        public Image itemImage;
        public TextMeshProUGUI itemTitle;
        public TextMeshProUGUI itemCaption;
        public GameObject parent;

        override public void HandleValueChanged(ItemHandler itemHandler) {
            if (itemHandler.activeItem == null || itemHandler.activeItem.data == null) {
                itemImage.sprite = null;
                itemImage.enabled = false;
                itemTitle.text = "N/A";
                itemCaption.text = "";
                parent.SetActive(false);
            } else {
                parent.SetActive(true);
                // TODO: fix this up
                itemImage.enabled = true;
                itemImage.sprite = itemHandler.activeItem.data.image;
                itemTitle.text = itemHandler.activeItem.data.name;
                itemCaption.text = "1/1";
            }
        }
    }
}