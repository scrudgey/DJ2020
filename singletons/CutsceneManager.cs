using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class CutsceneManager : Singleton<CutsceneManager> {
    public Dictionary<string, ScriptSceneLocation> worldLocations;
    public Dictionary<string, ScriptSceneCameraPosition> cameraLocations;
    public static Action<string> OnTrigger;
    Stack<Cutscene> cutscenesAwaitingFocus = new Stack<Cutscene>();
    Cutscene cutsceneWithFocus;

    List<Coroutine> runningCutscenes;
    public void InitializeSceneReferences() {
        OnTrigger = null;
        runningCutscenes = new List<Coroutine>();
        cutscenesAwaitingFocus = new Stack<Cutscene>();
        worldLocations = GameObject.FindObjectsOfType<ScriptSceneLocation>().ToDictionary(x => x.idn, x => x);
        cameraLocations = GameObject.FindObjectsOfType<ScriptSceneCameraPosition>().ToDictionary(x => x.idn, x => x);
    }

    public bool cutsceneIsRunning() {
        return cutsceneWithFocus != null;
    }
    public void StartCutscene(Cutscene cutscene) {
        runningCutscenes.Add(StartCoroutine(cutscene.Play()));
    }

    public void HandleTrigger(string idn) {
        OnTrigger?.Invoke(idn);
    }


    public void RequestFocus(Cutscene requester) {
        if (cutsceneWithFocus == null) {
            requester.hasFocus = true;
            cutsceneWithFocus = requester;

            requester.characterCamera = GameManager.I.characterCamera;
            requester.playerCharacterController = GameManager.I.playerCharacterController;
            requester.clearsighter = GameManager.I.clearsighterV4;
            requester.playerObject = GameManager.I.playerObject;

            GameManager.I.uiController.HideUI();
            AudioListener cameraListener = GameManager.I.characterCamera.GetComponent<AudioListener>();
            AudioListener playerListener = GameManager.I.playerObject.GetComponent<AudioListener>();
            playerListener.enabled = false;
            cameraListener.enabled = true;

            Time.timeScale = 0;
        } else {
            cutscenesAwaitingFocus.Push(requester);
        }
    }

    public void LeaveFocus(Cutscene requester) {
        if (cutsceneWithFocus == requester) {
            cutsceneWithFocus.hasFocus = false;
            cutsceneWithFocus = null;
        }
        if (cutscenesAwaitingFocus.Count > 0) {
            Cutscene nextCutscene = cutscenesAwaitingFocus.Pop();
            RequestFocus(nextCutscene);
        } else {
            GameManager.I.uiController.ShowUI();
            Time.timeScale = 1;
            AudioListener cameraListener = GameManager.I.characterCamera.GetComponent<AudioListener>();
            AudioListener playerListener = GameManager.I.playerObject.GetComponent<AudioListener>();
            cameraListener.enabled = false;
            playerListener.enabled = true;
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

    public static IEnumerator WaitForTrigger(string idn) {
        bool trigger = false;
        Action<string> callback = (string triggerId) => {
            trigger |= triggerId == idn;
        };
        CutsceneManager.OnTrigger += callback;
        while (!trigger) {
            yield return null;
        }
        CutsceneManager.OnTrigger -= callback;
    }

}

