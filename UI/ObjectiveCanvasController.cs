using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectiveCanvasController : MonoBehaviour {
    public Transform objectivesContainer;
    public GameObject objectiveIndicatorPrefab;
    public Canvas canvas;
    Coroutine showCoroutine;
    public Dictionary<Objective, ObjectiveIndicatorController> controllers;
    public void Start() {
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
        HandleObjectivesChange(data);
    }
    void OnDestroy() {
        GameManager.OnObjectivesChange -= HandleObjectivesChange;
    }
    public void HandleObjectivesChange(GameData data) {
        int index = 0;
        foreach (KeyValuePair<Objective, ObjectiveIndicatorController> kvp in controllers) {
            kvp.Value.Configure(kvp.Key, data, index);
            index += 1;
        }
        ShowCanvasCoroutine();
    }

    void ShowCanvasCoroutine() {
        if (showCoroutine == null) {
            showCoroutine = StartCoroutine(ShowCanvas());
        }
    }
    IEnumerator ShowCanvas() {
        float timer = 0f;
        canvas.enabled = true;
        while (timer < 5f) {
            timer += Time.unscaledDeltaTime;
            yield return null;
        }
        canvas.enabled = false;
        showCoroutine = null;
    }
}
