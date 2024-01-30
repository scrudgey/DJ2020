using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class InsertionPointSelector : MonoBehaviour {
    // public MissionPlanMapController controller;
    public MapMarkerData data;
    public Objective objective;
    public GameObject checkboxObject;
    public GameObject questionMarkObject;
    public Image dotImage;
    public Image checkImage;
    public TextMeshProUGUI text;
    public Button button;
    Action<InsertionPointSelector> callback;

    public void Configure(MapMarkerData data, Action<InsertionPointSelector> callback) {
        // this.controller = controller;
        this.data = data;
        this.callback = callback;
        text.text = data.markerName;
        questionMarkObject.SetActive(false);
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
    public void Configure(Objective objective, Action<InsertionPointSelector> callback, bool visibile) {
        text.text = objective.title;
        this.callback = callback;
        this.objective = objective;
        checkImage.enabled = false;
        checkboxObject.SetActive(false);

        if (visibile) {
            dotImage.gameObject.SetActive(true);
            questionMarkObject.SetActive(false);
        } else {
            dotImage.gameObject.SetActive(false);
            questionMarkObject.SetActive(true);
        }

    }
    public void Clicked() {
        // controller.ColumnItemCallback(this);
        callback?.Invoke(this);
    }

    public void Check(bool value) {
        checkImage.enabled = value;
    }
}
