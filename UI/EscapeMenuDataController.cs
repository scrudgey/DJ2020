using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class EscapeMenuDataController : MonoBehaviour {
    public GameObject dataEntryPrefab;
    public GameObject dataEntryHeaderPrefab;
    public Transform dataEntryContainer;
    public GraphIconReference graphIconReference;
    [Header("view")]
    public TextMeshProUGUI viewTitle;
    public Image viewIcon;
    public TextMeshProUGUI viewDescription;
    public void Initialize(GameData data) {
        ClearView();
        List<PayData> datas = data.playerState.payDatas.Concat(data.levelState.delta.levelAcquiredPaydata).ToList();
        PopulateKeyColumn(datas);
    }

    void PopulateKeyColumn(List<PayData> payDatas) {
        foreach (Transform child in dataEntryContainer) {
            Destroy(child.gameObject);
        }
        bool selectedInitialValue = false;
        foreach (IGrouping<PayData.DataType, PayData> group in payDatas.GroupBy(data => data.type)) {
            PayData.DataType key = group.Key;

            GameObject headerObj = GameObject.Instantiate(dataEntryHeaderPrefab);
            TextMeshProUGUI headerText = headerObj.GetComponentInChildren<TextMeshProUGUI>();
            headerText.text = $"{key}";
            headerObj.transform.SetParent(dataEntryContainer, false);

            foreach (PayData data in group) {
                GameObject obj = GameObject.Instantiate(dataEntryPrefab);

                DataEntryButton dataEntryButton = obj.GetComponent<DataEntryButton>();
                dataEntryButton.Configure(ClickCallback, data);

                obj.transform.SetParent(dataEntryContainer, false);
                if (!selectedInitialValue) {
                    selectedInitialValue = true;
                    ClickCallback(data);
                    Button button = obj.GetComponent<Button>();
                    button.Select();
                }
            }
        }
    }

    void ClickCallback(PayData data) {
        PopulateView(data);
    }

    void PopulateView(PayData data) {
        viewIcon.enabled = true;
        viewIcon.sprite = graphIconReference.DataSprite(data.type);

        viewTitle.text = $"{data.filename}";
        viewDescription.text = $"{data.type}";
    }

    void ClearView() {
        viewIcon.enabled = false;
        viewTitle.text = $"";
        viewDescription.text = $"";
    }
}
