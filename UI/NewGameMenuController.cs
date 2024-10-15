using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
public class NewGameMenuController : MonoBehaviour {
    public TitleController titleController;
    public AudioSource audioSource;
    public AudioClip blipSound;
    public TMP_InputField inputField;
    public GameObject skipTutorialCheckboxObject;
    public Toggle skipTutorialToggle;

    public void InputChangedCallback() {
        audioSource.PlayOneShot(blipSound);
    }
    public void Initialize() {
        int saveCount = CountSaveFiles();
        skipTutorialCheckboxObject.SetActive(saveCount > 0);
    }

    public void NewGameCallback() {
        // Debug.Log("newgame");

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
        int saveCount = CountSaveFiles();
        bool doTutorial = saveCount <= 0 || !skipTutorialToggle.isOn;
        GameManager.I.StartNewGame(filename, doTutorial);
    }

    int CountSaveFiles() {
        DirectoryInfo d = new DirectoryInfo(GameData.SaveGameRootDirectory());
        FileInfo[] Files = d.GetFiles(); //Getting Text files
        return Files
            .Where(file => file.Name != ".DS_Store")
            .Count();
    }
}
