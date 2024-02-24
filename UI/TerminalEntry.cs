using System.Collections;
using System.Collections.Generic;
using Easings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class TerminalEntry : MonoBehaviour {
    public TextMeshProUGUI text;
    public IEnumerator Write(Writeln input) {
        // text.text = input;
        text.text = "";
        return Toolbox.TypeText(text, input.prefix, input.content, typedInput: input.playerType);
    }
}
