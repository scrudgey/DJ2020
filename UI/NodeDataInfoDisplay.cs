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

    // TODO: support data stolen
    public void Configure(PayData data, bool isStolen) {
        if (isStolen) {
            ConfigureStolen();
        } else {
            ConfigureNormal(data);
        }

    }
    void ConfigureStolen() {
        filename.text = "empty";
        dataType.text = $"";
        valueamount.text = $"-";
        creditIndicator.SetActive(false);
        valueamount.gameObject.SetActive(false);
        icon.sprite = null;
        icon.enabled = false;

    }
    void ConfigureNormal(PayData data) {
        filename.text = data.filename;
        dataType.text = $"{data.type}";
        valueamount.text = $"{data.value}";
        creditIndicator.SetActive(data.type == PayData.DataType.pay);
        valueamount.gameObject.SetActive(data.type == PayData.DataType.pay);
        icon.enabled = true;
        icon.sprite = data.type switch {
            PayData.DataType.location => iconLocation,
            PayData.DataType.objective => iconObjective,
            PayData.DataType.password => iconPassword,
            PayData.DataType.pay => iconPay,
            PayData.DataType.personnel => iconPersonnel,
            _ => iconPay
        };

    }
}
