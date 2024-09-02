using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class CutsceneManager : Singleton<CutsceneManager> {
    public Dictionary<string, ScriptSceneLocation> worldLocations;
    public Dictionary<string, ScriptSceneCameraPosition> cameraLocations;
    public static Action<string> OnTrigger;
    public AudioListener cameraListener;
    public AudioListener playerListener;
    Stack<Cutscene> cutscenesAwaitingFocus = new Stack<Cutscene>();
    Cutscene cutsceneWithFocus;

    List<Coroutine> runningCutscenes;
    public void InitializeSceneReferences() {
        Debug.Log("[cutscene] clearing OnTrigger");
        OnTrigger = null;
        runningCutscenes = new List<Coroutine>();
        cutscenesAwaitingFocus = new Stack<Cutscene>();
        worldLocations = GameObject.FindObjectsOfType<ScriptSceneLocation>().ToDictionary(x => x.idn, x => x);
        cameraLocations = GameObject.FindObjectsOfType<ScriptSceneCameraPosition>().ToDictionary(x => x.idn, x => x);
    }

    public bool cutsceneIsRunning() {
        return cutsceneWithFocus != null;
    }
    public Cutscene runningCutscene() {
        return cutsceneWithFocus;
    }
    public void StartCutscene(Cutscene cutscene) {
        runningCutscenes.Add(StartCoroutine(cutscene.Play()));
    }

    public void HandleTrigger(string idn) {
        Debug.Log($"[cutscene manager] trigger: {idn}");
        OnTrigger?.Invoke(idn);
    }

    public IEnumerator RequestFocus(Cutscene requester) {
        // Debug.Log($"cutsceneWithFocus: {cutsceneWithFocus}");
        if (cutsceneWithFocus == null) {
            // Debug.Log("grant focus immediately");
            requester.hasFocus = true;
            cutsceneWithFocus = requester;

            requester.characterCamera = GameManager.I.characterCamera;
            requester.playerCharacterController = GameManager.I.playerCharacterController;
            requester.playerObject = GameManager.I.playerObject;

            while (GameManager.I.uiController == null) {
                yield return null;
            }

            GameManager.I.uiController.HideUI(hideKeySelect: false);
            cameraListener = GameManager.I.characterCamera.GetComponent<AudioListener>();
            playerListener = GameManager.I.playerObject.GetComponent<AudioListener>();
            // playerListener.enabled = false;
            // cameraListener.enabled = true;

            Time.timeScale = 0;
        } else {
            cutscenesAwaitingFocus.Push(requester);
            // Debug.Log($"request focus: wait in line {cutscenesAwaitingFocus.Count}");
        }
    }

    public IEnumerator LeaveFocus(Cutscene requester) {
        if (cutsceneWithFocus == null) {
            yield return null;
        } else {
            if (cutsceneWithFocus == requester) {
                bool focusCutsceneIsRunning = cutsceneWithFocus.isPlaying;

                cutsceneWithFocus.hasFocus = false;
                cutsceneWithFocus = null;

                if (cutscenesAwaitingFocus.Count > 0) {
                    Cutscene nextCutscene = cutscenesAwaitingFocus.Pop();
                    yield return RequestFocus(nextCutscene);
                } else {
                    // if (!focusCutsceneIsRunning)
                    GameManager.I.uiController.ShowUI(hideCutsceneText: false);
                    Time.timeScale = 1;
                    AudioListener cameraListener = GameManager.I.characterCamera.GetComponent<AudioListener>();
                    AudioListener playerListener = GameManager.I.playerObject.GetComponent<AudioListener>();
                    cameraListener.enabled = false;
                    playerListener.enabled = true;
                }
            }
        }
    }

    public GameObject SpawnObject(string locationId, GameObject prefab, string lookAtId) {
        Vector3 lookAtPoint = CutsceneManager.I.worldLocations[lookAtId].transform.position;
        GameObject obj = null;
        if (worldLocations.ContainsKey(locationId)) {
            ScriptSceneLocation location = worldLocations[locationId];
            Quaternion rotation = Quaternion.LookRotation(lookAtPoint - location.transform.position, Vector3.up);
            obj = GameObject.Instantiate(prefab, location.transform.position, rotation) as GameObject;
        } else {
            Debug.LogError($"[cutscene] error spawning object: location not found: {locationId}");
        }

        foreach (Collider collider in obj.GetComponentsInChildren<Collider>()) {
            GameManager.I.clearsighterV4.GetDynamicHandler(collider, obj.transform);
        }

        return obj;
    }

    public GameObject SpawnNPC(string locationId, NPCTemplate template) {
        Vector3 location = CutsceneManager.I.worldLocations[locationId].transform.position;
        GameObject obj = NPCSpawnPoint.SpawnNPC(template, location, null, null);

        foreach (Collider collider in obj.GetComponentsInChildren<Collider>()) {
            GameManager.I.clearsighterV4.GetDynamicHandler(collider, obj.transform);
        }

        return obj;
    }

    public void NPCLookAt(CharacterController character, string locationId) {
        Vector3 location = CutsceneManager.I.worldLocations[locationId].transform.position;
        Vector3 direction = location - character.transform.position;
        direction.y = 0;
        // character.direction = direction;
        character.lookAtDirection = direction;
    }

    public void NPCClearPoints(CharacterController character) {
        SphereRobotAI ai = character.GetComponent<SphereRobotAI>();
        ai.ChangeState(new SphereClearPointsState(ai, character, FindObjectsOfType<ClearPoint>(), speed: 0.5f));
    }
}

