using System.Collections;
using System.Collections.Generic;
using KinematicCharacterController;
using UnityEngine;
public class NPCInitializer : MonoBehaviour {
    public NPCTemplate template;
    void Start() {
        StartCoroutine(WaitToInitialize());
    }
    IEnumerator WaitToInitialize() {
        WaitForEndOfFrame waiter = new WaitForEndOfFrame();
        while (GameManager.I.isLoadingLevel) {
            yield return waiter;
        }
        InitializeNPC();
    }
    void InitializeNPC() {
        CharacterCamera cam = GameObject.FindObjectOfType<CharacterCamera>();

        CharacterController controller = GetComponentInChildren<CharacterController>();
        // KinematicCharacterMotor motor = GetComponentInChildren<KinematicCharacterMotor>();
        // SphereRobotAI ai = GetComponentInChildren<SphereRobotAI>();

        controller.OrbitCamera = cam;
        // ai.patrolRoute = route;

        // should be part of apply state?
        // ai.etiquettes = template.etiquettes;
        // ai.portrait = template.portrait;
        // motor.SetPosition(transform.position, bypassInterpolation: true);
        ApplyNPCState(template, gameObject);
        // ai.Initialize();
    }

    void ApplyNPCState(NPCTemplate template, GameObject npcObject) {
        NPCState state = NPCState.Instantiate(template);
        state.ApplyState(npcObject);
    }
}
