using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;


namespace UI {
    public class ItemUIHandler : IBinder<ItemHandler> {
        // public ItemHandler target { get; set; }
        public Image itemImage;
        public TextMeshProUGUI itemTitle;
        public TextMeshProUGUI itemCaption;
        public GameObject parent;
        public TextMeshProUGUI buttonText;
        // public InputAction useItem;
        public InputActionReference actionReference;
        public int bindingIndex;
        override public void HandleValueChanged(ItemHandler itemHandler) {
            InputAction action = actionReference.action;
            var displayString = string.Empty;
            var deviceLayoutName = default(string);
            var controlPath = default(string);
            if (action != null) {
                buttonText.text = action.GetBindingDisplayString(bindingIndex, out deviceLayoutName, out controlPath);
            }
            // Debug.Log(buttonText.text);
            if (itemHandler.activeItem == null || itemHandler.activeItem.template == null) {
                itemImage.sprite = null;
                itemImage.enabled = false;
                itemTitle.text = "N/A";
                itemCaption.text = "";
                parent.SetActive(false);
            } else {
                parent.SetActive(true);
                // TODO: fix this up
                itemImage.enabled = true;
                itemImage.sprite = itemHandler.activeItem.template.image;
                itemTitle.text = itemHandler.activeItem.template.name;
                itemCaption.text = "1/1";
            }
        }
    }
}