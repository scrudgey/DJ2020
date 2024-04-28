using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class SceneExit : Interactive {
    public string toSceneName;
    override public void Start() {
        base.Start();
        Scene myscene = SceneManager.GetSceneByName(toSceneName);
        if (myscene == null) {
            Debug.LogError($"Can't find exit scene named {toSceneName}!");
        }
    }
    public override ItemUseResult DoAction(Interactor interactor) {

        return ItemUseResult.Empty() with {
            changeScene = true,
            toSceneName = toSceneName
        };
    }
    public override string ResponseString() {
        return $"used exit";
    }
}
