using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NodeDataInfoDisplay : MonoBehaviour {
    public Image icon;
    public TextMeshProUGUI filename;
    public TextMeshProUGUI dataType;
    public TextMeshProUGUI valueamount;
    public GameObject creditIndicator;
    [Header("datatype icons")]
    public Sprite iconPay;
    public Sprite iconPersonnel;
    public Sprite iconPassword;
    public Sprite iconLocation;
    public Sprite iconObjective;

    public void Configure(PayData data) {
        filename.text = data.filename;
        dataType.text = $"{data.type}";
        valueamount.tag = $"{data.value}";
        creditIndicator.SetActive(data.type == PayData.DataType.pay);
        valueamount.gameObject.SetActive(data.type == PayData.DataType.pay);

        // TODO: set icons
        // TODO: support data stolen
    }
}
