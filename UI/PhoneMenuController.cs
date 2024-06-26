using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class PhoneMenuController : MonoBehaviour {
    public Canvas myCanvas;
    public GameObject UIEditorCamera;
    public RectTransform myRect;

    public Transform phoneButtonContainer;
    public GameObject phoneButtonPrefab;
    public WorldmapView worldmapView;
    [Header("sounds")]
    public AudioClip[] closeSounds;
    public AudioClip[] openSounds;

    void Awake() {
        myCanvas.enabled = false;
        DestroyImmediate(UIEditorCamera);
    }
    public void Initialize() {
        GameManager.I.PlayUISound(openSounds);
        foreach (Transform child in phoneButtonContainer) {
            Destroy(child.gameObject);
        }
        foreach (FenceData data in GameManager.I.gameData.fenceData) {
            if (data.fence.isRemote) {
                CreateButton(data);
            }
        }
        worldmapView.ShowWorldText();
        myCanvas.enabled = true;
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
        // GameManager.I.CloseMenu();
        GameManager.I.PlayUISound(closeSounds);
        StartCoroutine(Toolbox.CloseMenu(myRect));
    }
}
