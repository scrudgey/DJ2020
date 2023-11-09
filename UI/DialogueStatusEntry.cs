using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// ⁙

public class DialogueStatusEntry : MonoBehaviour {
    public TextMeshProUGUI dotText;
    public TextMeshProUGUI contentText;
    public Color green;
    public Color yellow;
    public Color red;
    public Color white;
    public void Initialize(int alarmCount, string content) {
        contentText.text = content;
        dotText.text = alarmCount switch {
            < -3 => "⁘",
            -3 => "•••",
            -2 => "••",
            -1 => "•",
            0 => "-",
            1 => "!",
            2 => "!!",
            3 => "!!!",
            > 3 => "!!!"
        };
        if (alarmCount <= 0) {
            dotText.color = green;
        } else {
            dotText.color = red;
        }
    }

    public void InitializeNumeric(int alarmCount, string content, bool plain = false) {
        contentText.text = content;
        if (plain) {
            dotText.text = $"{alarmCount}";
            dotText.color = white;
        } else if (alarmCount < 0) {
            dotText.text = $"{alarmCount}";
            dotText.color = red;
        } else if (alarmCount > 0) {
            dotText.text = $"+{alarmCount}";
            dotText.color = green;
        } else {
            dotText.text = $"+0";
            dotText.color = white;
        }
    }
}
