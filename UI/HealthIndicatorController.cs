using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class HealthIndicatorController : IBinder<CharacterHurtable> {
    public Transform topPipsBar;
    public Transform bottomPipsBar;
    public TextMeshProUGUI valueText;
    public TextMeshProUGUI maxText;
    public List<Image> topPipImages;
    public List<Image> bottomPipImages;
    public Color livePipColor;
    public Color deadPipColor;
    void Start() {
        topPipImages = new List<Image>();
        bottomPipImages = new List<Image>();
        foreach (Transform child in topPipsBar) {
            topPipImages.Add(child.GetComponentInChildren<Image>());
        }
        foreach (Transform child in bottomPipsBar) {
            bottomPipImages.Add(child.GetComponentInChildren<Image>());
        }
        for (int i = 0; i < 9; i++) {
            topPipImages[i].transform.SetSiblingIndex(i);
        }
        for (int i = 0; i < 9; i++) {
            bottomPipImages[i].transform.SetSiblingIndex(i);
        }
    }
    public override void HandleValueChanged(CharacterHurtable characterHurtable) {
        valueText.text = ((int)characterHurtable.health).ToString();
        maxText.text = ((int)characterHurtable.fullHealthAmount).ToString();

        // 500 health = 10 pips
        // 1 pip per 50 health

        int livePips = (int)(characterHurtable.health / 50);
        int maxpips = (int)(characterHurtable.fullHealthAmount / 50);

        int topMaxPips = Mathf.Min(10, maxpips);
        int bottomMaxPips = (topMaxPips > 10) ? maxpips % 10 : 0;

        int topLivePips = Mathf.Min(10, livePips);
        int bottomLivePips = (topMaxPips > 10) ? livePips % 10 : 0;

        // Debug.Log($"live pips: {topLivePips} {bottomLivePips}");
        // Debug.Log($"max pips: {topMaxPips} {bottomMaxPips}");
        // Debug.Log($"{toppips} {bottomPips}");
        for (int i = 0; i <= 9; i++) {
            topPipImages[i].enabled = (i < topMaxPips);
            topPipImages[i].color = (i < topLivePips) ? livePipColor : deadPipColor;
        }
        for (int i = 0; i <= 9; i++) {
            // Debug.Log($"{bottomPipImages[i].name} {i} {(i < bottomPips)}");
            bottomPipImages[i].enabled = (i < bottomMaxPips);
            bottomPipImages[i].color = (i < bottomLivePips) ? livePipColor : deadPipColor;
        }
    }
}
