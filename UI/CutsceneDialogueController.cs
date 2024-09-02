using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class CutsceneDialogueController : MonoBehaviour {
    public enum Location { bottom, top }
    public RectTransform mainRect;
    public StoreDialogueController storeDialogueController;
    public GameObject continueButton;
    Action continueAction;
    bool doContinue;
    public void SetLocation(Location location) {
        switch (location) {
            case Location.bottom:
                mainRect.localPosition = new Vector3(0f, -169, 0);
                break;
            case Location.top:
                mainRect.localPosition = new Vector3(0f, 500f, 0f);
                break;
        }
    }
    public IEnumerator Initialize(string name, Sprite portrait, string content, UIController uIController, Location location = Location.bottom) {
        SetLocation(location);
        uIController.cutsceneDialogueEnabled = true;
        continueButton.SetActive(false);
        storeDialogueController.SetImages(portrait);
        storeDialogueController.Initialize(GameManager.I.gameData.filename, name);
        yield return storeDialogueController.CutsceneDialogue(content);
        yield return WaitForContinue();
        GameManager.I.uiController.HideCutsceneDialogue();
        uIController.cutsceneDialogueEnabled = false;
    }

    void ContinueCutscene() {
        doContinue = true;
    }
    IEnumerator WaitForContinue() {
        doContinue = false;
        continueAction = ContinueCutscene;
        continueButton.SetActive(true);
        yield return new WaitUntil(() => doContinue);
        doContinue = false;
    }
    public void ContinueButtonCallback() {
        continueAction?.Invoke();
    }
}
