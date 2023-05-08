using System.Collections;
using System.Collections.Generic;
using Easings;
using UnityEngine;
public class ObjectiveCanvasController : MonoBehaviour {
    public Transform objectivesContainer;
    public GameObject objectiveIndicatorPrefab;
    public Canvas canvas;
    Coroutine showCoroutine;
    public Dictionary<Objective, ObjectiveIndicatorController> controllers;
    public RectTransform containerRect;
    public void Initialize() {
        foreach (Transform child in objectivesContainer) {
            if (child.gameObject.name == "image") continue;
            Destroy(child.gameObject);
        }
    }

    public void Initialize(GameData gameData) {
        controllers = new Dictionary<Objective, ObjectiveIndicatorController>();
        foreach (Objective objective in gameData.levelState.template.objectives) {
            GameObject obj = GameObject.Instantiate(objectiveIndicatorPrefab);
            obj.transform.SetParent(objectivesContainer, false);
            ObjectiveIndicatorController controller = obj.GetComponent<ObjectiveIndicatorController>();
            controllers[objective] = controller;
        }
        Bind(gameData);
    }

    public void Bind(GameData data) {
        GameManager.OnObjectivesChange += HandleObjectivesChange;
        HandleObjectivesChange(data, null);
    }
    void OnDestroy() {
        GameManager.OnObjectivesChange -= HandleObjectivesChange;
    }
    public void HandleObjectivesChange(GameData data, Dictionary<Objective, ObjectiveStatus> changedStatuses) {
        int index = 0;
        foreach (KeyValuePair<Objective, ObjectiveIndicatorController> kvp in controllers) {
            kvp.Value.Configure(kvp.Key, data, index);
            index += 1;
        }
        ShowCanvasCoroutine();

        if (changedStatuses != null) {
            foreach (KeyValuePair<Objective, ObjectiveStatus> kvp in changedStatuses) {
                Debug.Log($"changed status {kvp.Key} {kvp.Value}");
                controllers[kvp.Key].IndicateValueChanged();
            }
        }
    }

    void ShowCanvasCoroutine() {
        if (showCoroutine == null) {
            showCoroutine = StartCoroutine(ShowCanvas());
        }
    }
    IEnumerator ShowCanvas() {
        float timer = 0f;
        canvas.enabled = true;
        containerRect.anchoredPosition = new Vector2(containerRect.rect.width, 0f);
        float duration = 0.5f;
        float hangtime = 4f;
        while (timer < duration) {
            timer += Time.unscaledDeltaTime;
            float x = (float)PennerDoubleAnimation.ExpoEaseOut(timer, containerRect.rect.width, -containerRect.rect.width - 10, duration);
            containerRect.anchoredPosition = new Vector2(x, 0f);
            yield return null;
        }
        containerRect.anchoredPosition = new Vector2(-10, 0f);
        timer = 0f;
        while (timer < hangtime) {
            timer += Time.unscaledDeltaTime;
            yield return null;
        }
        timer = 0f;
        while (timer < duration) {
            timer += Time.unscaledDeltaTime;
            float x = (float)PennerDoubleAnimation.ExpoEaseIn(timer, -10, containerRect.rect.width + 10, duration);
            containerRect.anchoredPosition = new Vector2(x, 0f);
            yield return null;
        }
        canvas.enabled = false;
        showCoroutine = null;
    }
}
