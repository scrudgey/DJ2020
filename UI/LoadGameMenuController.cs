using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        List<GameData> saveDatas = Files
            .Where(file => file.Name != ".DS_Store")
            .Select(file => GameData.Load(file.Name))
            .OrderBy(data => data.lastPlayedTime)
            .Reverse()
            .ToList();

        foreach (GameData saveData in saveDatas) {
            // Debug.Log(file.Name);
            GameObject loadButton = GameObject.Instantiate(loadButtonPrefab);
            SaveGameSelectorButton saveGameSelectorButton = loadButton.GetComponent<SaveGameSelectorButton>();
            saveGameSelectorButton.Initialize(this, saveData);
            loadButton.transform.SetParent(loadButtonContainer);
        }
    }
    public void LoadButtonCallback(GameData data) {
        GameManager.I.LoadGame(data);
    }

    public void CancelButtonCallback() {
        // close menu
        titleController.CloseLoadMenu();
    }
}
