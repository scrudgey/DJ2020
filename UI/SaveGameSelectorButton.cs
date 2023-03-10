using System;
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
        TimeSpan time = TimeSpan.FromSeconds(data.timePlayedInSeconds);
        lastPlayedText.text = $"Last played: {data.lastPlayedTime.ToLongDateString()}";
        timePlayedText.text = $"Time played: {time.ToString(@"hh\:mm\:ss")}";
    }
    public void ClickedCallback() {
        controller.LoadButtonCallback(gameData);
    }
}
