using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class SuspicionStatusRecordIndicatorHandler : MonoBehaviour {
    public SuspicionRecord suspicionRecord;
    public TextMeshProUGUI dotText;
    public TextMeshProUGUI contentText;
    public Color green;
    public Color yellow;
    public Color red;
    public void Configure(SuspicionRecord record) {
        this.suspicionRecord = record;
        dotText.color = record.suspiciousness switch {
            Suspiciousness.normal => green,
            Suspiciousness.suspicious => yellow,
            Suspiciousness.aggressive => red,
            _ => green
        };
        contentText.text = record.content;
    }
}
