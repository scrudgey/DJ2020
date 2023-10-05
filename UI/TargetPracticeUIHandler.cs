using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public record TargetHitData {
    public int points;
    public float distance;
}
public class TargetPracticeUIHandler : MonoBehaviour {
    public Canvas canvas;
    public TextMeshProUGUI shotsText;
    public TextMeshProUGUI accuracyText;
    public TextMeshProUGUI totalPointsText;
    public TextMeshProUGUI avgPointsText;
    public TextMeshProUGUI totalDistanceText;
    public TextMeshProUGUI avgDistanceText;
    public TextMeshProUGUI pointText;
    public TextMeshProUGUI distanceText;

    int shots;
    int hits;
    float accuracy;
    int points;
    // float averagePoints;
    float distance;

    float totalDistance;
    int totalPoints;
    // float averageDistance;

    public static Action OnShotFired;
    public static Action<TargetHitData> OnTargetHit;

    void Start() {
        OnShotFired += HandleShotFired;
        OnTargetHit += HandleTargetHit;
    }
    void OnDestroy() {
        OnShotFired -= HandleShotFired;
        OnTargetHit -= HandleTargetHit;
    }

    public void ResetButtonCallback() {
        shots = 0;
        hits = 0;
        accuracy = 0;
        points = 0;
        distance = 0;
        UpdateView();
    }

    void UpdateView() {
        float accuracy = (float)hits / (float)shots;
        float averageDistance = totalDistance / (float)hits;
        float averagePoints = (float)totalPoints / (float)hits;

        shotsText.text = $"{shots}";
        accuracyText.text = $"{accuracy.ToString("f2")}";

        pointText.text = $"{points}";
        distanceText.text = $"{distance.ToString("f2")}";

        totalPointsText.text = $"{totalPoints}";
        avgPointsText.text = $"{averagePoints.ToString("f2")}";
        totalDistanceText.text = $"{(int)totalDistance}";
        avgDistanceText.text = $"{averageDistance.ToString("f2")}";
    }

    void HandleShotFired() {
        shots += 1;
        UpdateView();
    }
    void HandleTargetHit(TargetHitData data) {
        totalPoints += data.points;
        totalDistance += data.distance * 10f;
        distance = data.distance * 10f;
        points = data.points;
        hits += 1;
        UpdateView();
    }
}
