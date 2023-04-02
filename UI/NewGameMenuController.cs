using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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

        // TODO: verify save name does not exist
        string filename = inputField.text;
        string path = GameData.SaveGamePath(filename);
        if (filename == "") {
            titleController.ShowAlert($"Invalid save file name!");
        } else if (File.Exists(path)) {
            titleController.ShowAlert($"Save file {filename} already exists!");
            return;
        }

        StartNewGame(filename);
    }
    public void CancelCallback() {
        titleController.CloseNewGameMenu();
    }

    void StartNewGame(string filename) {
        // todo: check for collision with existing save file

        GameManager.I.StartNewGame(filename);
    }
}
