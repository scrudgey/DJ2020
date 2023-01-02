using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class InsertionPointSelector : MonoBehaviour {
    public MissionPlanMapController controller;
    public MapMarkerData data;
    public GameObject checkboxObject;
    public Image dotImage;
    public Image checkImage;
    public TextMeshProUGUI text;
    public Button button;

    public void Configure(MissionPlanMapController controller, MapMarkerData data) {
        this.controller = controller;
        this.data = data;

        text.text = data.markerName;

        switch (data.markerType) {
            case MapMarkerData.MapMarkerType.decor:
            case MapMarkerData.MapMarkerType.objective:
            case MapMarkerData.MapMarkerType.pointOfInterest:
            case MapMarkerData.MapMarkerType.guard:
                checkboxObject.SetActive(false);
                break;
            case MapMarkerData.MapMarkerType.insertionPoint:
            case MapMarkerData.MapMarkerType.extractionPoint:
                dotImage.gameObject.SetActive(false);
                checkImage.enabled = false;
                break;
        }

    }
    public void Clicked() {
        controller.ColumnItemCallback(this);
    }

    public void Check(bool value) {
        checkImage.enabled = value;
    }
}
