using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class SaveGameSelectorButton : MonoBehaviour {

    public Image headSprite;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI lastPlayedText;
    public TextMeshProUGUI timePlayedText;
    GameData gameData;

    LoadGameMenuController controller;
    public void Initialize(LoadGameMenuController controller, GameData data) {
        this.controller = controller;
        this.titleText.text = data.filename;
        this.gameData = data;
    }
    public void ClickedCallback() {
        controller.LoadButtonCallback(gameData);
    }
}
