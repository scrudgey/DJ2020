using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class PayDataShopController : MonoBehaviour {

    public GameObject UIEditorCamera;
    public RectTransform bottomRect;
    public AudioSource audioSource;
    public StoreDialogueController storeDialogueController;

    [Header("data view")]
    public TextMeshProUGUI valueText;
    public TextMeshProUGUI playerCreditText;
    public TextMeshProUGUI filenameText;
    public TextMeshProUGUI fileContentText;
    public Button sellButton;

    [Header("data list")]
    public GameObject paydataButtonPrefab;
    public Transform payDataButtonsContainer;
    public GameObject emptyInventoryIndicator;

    [Header("sounds")]
    public AudioClip[] sellSound;
    public AudioClip[] selectDataSound;
    public AudioClip[] discloseBottomSound;


    DataFileButton selectedButton;

    void Awake() {
        DestroyImmediate(UIEditorCamera);
        bottomRect.sizeDelta = new Vector2(1f, 0f);
    }
    public void Initialize() {
        storeDialogueController.Initialize(GameManager.I.gameData.filename, "Stacks");

        storeDialogueController.SetShopownerDialogue("I can move your wares on the Tri/Net data exchange, for a price.");
        StartCoroutine(Toolbox.OpenStore(bottomRect, audioSource, discloseBottomSound));
        ClearDataButtons();
        ClearSaleData();
        PopulateDataButtons();
        SetPlayerCredits();
    }

    void ClearDataButtons() {
        foreach (Transform child in payDataButtonsContainer) {
            if (child.name == "empty") continue;
            Destroy(child.gameObject);
        }
    }
    void SetPlayerCredits() {
        playerCreditText.text = $"{GameManager.I.gameData.playerState.credits}";
    }
    void PopulateDataButtons() {
        foreach (PayData data in GameManager.I.gameData.playerState.payDatas) {
            CreateDataButton(data);
        }
        emptyInventoryIndicator.SetActive(GameManager.I.gameData.playerState.payDatas.Count == 0);
    }
    void CreateDataButton(PayData data) {
        GameObject obj = GameObject.Instantiate(paydataButtonPrefab);
        obj.transform.SetParent(payDataButtonsContainer, false);
        DataFileButton button = obj.GetComponent<DataFileButton>();
        button.Initialize(data, DataButtonCallback);
    }

    void DataButtonCallback(DataFileButton button) {
        selectedButton = button;
        sellButton.interactable = true;
        filenameText.text = button.payData.filename;
        fileContentText.text = button.payData.content.text;
        valueText.text = $"{button.payData.value}";
        Toolbox.RandomizeOneShot(audioSource, selectDataSound);
    }

    void ClearSaleData() {
        selectedButton = null;
        sellButton.interactable = false;
        filenameText.text = "";
        fileContentText.text = "";
        valueText.text = "";
    }

    public void SellButtonCallback() {
        Debug.Log("SELL");
        Toolbox.RandomizeOneShot(audioSource, sellSound);
        GameManager.I.gameData.playerState.payDatas.Remove(selectedButton.payData);
        GameManager.I.gameData.playerState.credits += selectedButton.payData.value;
        ClearDataButtons();
        ClearSaleData();
        PopulateDataButtons();
        SetPlayerCredits();
    }
    public void DoneButtonCallback() {
        GameManager.I.CloseMenu();
    }
}
