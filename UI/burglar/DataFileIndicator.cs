using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DataFileIndicator : MonoBehaviour {
    public TextMeshProUGUI filenameText;
    public Image dataIcon;

    public void Initialize(PayData payData) {
        if (payData == null) {
            filenameText.text = "";
            SetIconVisibility(false);
        } else {
            filenameText.text = payData.filename;
        }
    }

    public void SetIconVisibility(bool visibility) {
        dataIcon.enabled = visibility;
    }
}
