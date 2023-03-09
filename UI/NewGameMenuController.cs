using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NewGameMenuController : MonoBehaviour {
    public TitleController titleController;
    public AudioSource audioSource;
    public AudioClip blipSound;
    public TMP_InputField inputField;

    public void InputChangedCallback() {
        audioSource.PlayOneShot(blipSound);
    }
    public void NewGameCallback() {
        Debug.Log("newgame");
        StartNewGame(inputField.text);
    }
    public void CancelCallback() {
        titleController.CloseNewGameMenu();
    }

    void StartNewGame(string filename) {
        // todo: check for collision with existing save file
        GameData data = GameData.TestInitialData() with {
            filename = filename
        };

        GameManager.I.gameData = data;

        GameManager.I.SaveGameData();

        GameManager.I.ReturnToApartment();
    }
}
