using System.Collections;
using System.Collections.Generic;
using KinematicCharacterController;
using UnityEngine;
public class NPCInitializer : MonoBehaviour {
    public NPCTemplate template;
    void Start() {
        StartCoroutine(Toolbox.WaitForSceneLoadingToFinish(InitializeNPC));
    }
    void InitializeNPC() {
        CharacterCamera cam = GameObject.FindObjectOfType<CharacterCamera>();
        CharacterController controller = GetComponentInChildren<CharacterController>();
        LegsAnimation legsAnimation = GetComponentInChildren<LegsAnimation>();
        legsAnimation.characterCamera = cam;
        controller.OrbitCamera = cam;
        ApplyNPCState(template, gameObject);
    }

    void ApplyNPCState(NPCTemplate template, GameObject npcObject) {
        NPCState state = NPCState.Instantiate(template);
        state.ApplyState(npcObject);
    }
}
