using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Easings;
using UnityEngine;
public class ObjectiveCanvasController : MonoBehaviour {
    // public Transform objectivesContainer;
    // public OverlayHandler overlayHandler;
    // public GameObject objectiveIndicatorPrefab;
    // public Canvas canvas;
    // Coroutine showCoroutine;
    // public Dictionary<ObjectiveDelta, ObjectiveIndicatorController> controllers;
    // public RectTransform containerRect;
    public TerminalAnimation terminalAnimation;
    public Color successColor;
    public Color failColor;
    public Color inProgressColor;


    public void Initialize(GameData gameData) {
        // foreach (Transform child in objectivesContainer) {
        //     if (child.gameObject.name == "image") continue;
        //     Destroy(child.gameObject);
        // }
        // controllers = new Dictionary<ObjectiveDelta, ObjectiveIndicatorController>();
        // foreach (ObjectiveDelta objective in gameData.levelState.delta.AllObjectives()) {
        //     GameObject obj = GameObject.Instantiate(objectiveIndicatorPrefab);
        //     obj.transform.SetParent(objectivesContainer, false);
        //     ObjectiveIndicatorController controller = obj.GetComponent<ObjectiveIndicatorController>();
        //     controllers[objective] = controller;
        // }
        Bind();
    }

    public void Bind() {
        GameManager.OnObjectivesChange += HandleObjectivesChange;
        terminalAnimation.Clear();
        // HandleObjectivesChange(new List<ObjectiveDelta>(), null);
    }
    void OnDestroy() {
        GameManager.OnObjectivesChange -= HandleObjectivesChange;
    }
    public void HandleObjectivesChange(List<ObjectiveDelta> allObjectives, ObjectiveDelta changedStatuses, bool optional) {
        // int index = 0;
        // foreach (KeyValuePair<ObjectiveDelta, ObjectiveIndicatorController> kvp in controllers) {
        //     kvp.Value.Configure(kvp.Key, index);
        //     index += 1;
        // }
        // ShowCanvasCoroutine();

        if (changedStatuses != null) {
            Writeln[] writes = allObjectives.Select(delta => {
                Color color = delta.status switch {
                    ObjectiveStatus.inProgress => inProgressColor,
                    ObjectiveStatus.complete => successColor,
                    ObjectiveStatus.failed => failColor,
                    _ => inProgressColor
                };
                bool doFlash = changedStatuses == delta;
                string title = optional ? $"[optional] {delta.template.title}" : delta.template.title;
                return new Writeln("", $"{title}:\t\t{delta.status}", color) {
                    destroyAfter = 5f,
                    flash = doFlash
                };
            }).ToArray();

            terminalAnimation.DoWriteMany(writes);
            // terminalAnimation.DoWriteMany(new Writeln("", $"{changedStatuses.template.title}: {changedStatuses.status}") {
            //     destroyAfter = 3f
            // });
        }
    }

    // void ShowCanvasCoroutine() {
    //     if (showCoroutine == null) {
    //         showCoroutine = StartCoroutine(ShowCanvas());
    //     }
    // }
    // IEnumerator ShowCanvas() {
    //     overlayHandler.ShowInfoPane(null); // hide info pane
    //     float timer = 0f;
    //     canvas.enabled = true;
    //     containerRect.anchoredPosition = new Vector2(containerRect.rect.width, 0f);
    //     float duration = 0.5f;
    //     float hangtime = 4f;
    //     while (timer < duration) {
    //         timer += Time.unscaledDeltaTime;
    //         float x = (float)PennerDoubleAnimation.ExpoEaseOut(timer, containerRect.rect.width, -containerRect.rect.width - 10, duration);
    //         containerRect.anchoredPosition = new Vector2(x, 0f);
    //         yield return null;
    //     }
    //     containerRect.anchoredPosition = new Vector2(-10, 0f);
    //     timer = 0f;
    //     while (timer < hangtime) {
    //         timer += Time.unscaledDeltaTime;
    //         yield return null;
    //     }
    //     timer = 0f;
    //     while (timer < duration) {
    //         timer += Time.unscaledDeltaTime;
    //         float x = (float)PennerDoubleAnimation.ExpoEaseIn(timer, -10, containerRect.rect.width + 10, duration);
    //         containerRect.anchoredPosition = new Vector2(x, 0f);
    //         yield return null;
    //     }
    //     canvas.enabled = false;
    //     showCoroutine = null;
    // }
}
