using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class PhoneMenuController : MonoBehaviour {
    public GameObject UIEditorCamera;

    public Transform phoneButtonContainer;
    public GameObject phoneButtonPrefab;
    public WorldmapView worldmapView;

    void Awake() {
        DestroyImmediate(UIEditorCamera);
    }
    public void Initialize() {
        foreach (Transform child in phoneButtonContainer) {
            Destroy(child.gameObject);
        }
        foreach (FenceData data in GameManager.I.gameData.fenceData) {
            if (data.fence.isRemote) {
                CreateButton(data);
            }
        }
        worldmapView.ShowWorldText();
    }

    PhoneButton CreateButton(FenceData fenceData) {
        GameObject obj = GameObject.Instantiate(phoneButtonPrefab);
        obj.transform.SetParent(phoneButtonContainer, false);
        PhoneButton button = obj.GetComponent<PhoneButton>();
        button.Initialize(fenceData, this);
        return button;
    }

    public void ButtonCallback(PhoneButton phoneButton) {
        GameManager.I.ShowShopMenu(StoreType.loot, phoneButton.data.fence);
    }
    public void ButtonMouseOver(PhoneButton phoneButton) {
        worldmapView.HighlightPoint(3);
    }
    public void ButtonMouseExit() {
        worldmapView.StopHighlight();
    }
    public void DoneMenuClicked() {
        GameManager.I.CloseMenu();
    }
}
