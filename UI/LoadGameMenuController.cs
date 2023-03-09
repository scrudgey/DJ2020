using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
public class LoadGameMenuController : MonoBehaviour {
    public TitleController titleController;
    public GameObject loadButtonPrefab;
    public Transform loadButtonContainer;

    public void Initialize() {
        PopulateSaveGameList();
    }

    void PopulateSaveGameList() {
        foreach (Transform child in loadButtonContainer) {
            Destroy(child.gameObject);
        }
        GameData.CreateSaveGameFolderIfMissing();

        // populate buttons
        DirectoryInfo d = new DirectoryInfo(GameData.SaveGameRootDirectory());

        FileInfo[] Files = d.GetFiles(); //Getting Text files

        foreach (FileInfo file in Files) {
            Debug.Log(file.Name);
            if (file.Name == ".DS_Store") continue;
            Debug.Log($"loading {file.FullName}");
            GameData saveData = GameData.Load(file.Name);
            GameObject loadButton = GameObject.Instantiate(loadButtonPrefab);
            SaveGameSelectorButton saveGameSelectorButton = loadButton.GetComponent<SaveGameSelectorButton>();
            saveGameSelectorButton.Initialize(this, saveData);
            loadButton.transform.SetParent(loadButtonContainer);
        }
    }
    public void LoadButtonCallback(GameData data) {
        // handle load
        Debug.Log($"load {data.filename}");
        GameManager.I.gameData = data;
        GameManager.I.CloseMenu();
        GameManager.I.ReturnToApartment();
    }

    public void CancelButtonCallback() {
        // close menu
        titleController.CloseLoadMenu();
    }
}
