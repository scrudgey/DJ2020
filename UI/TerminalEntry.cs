using System.Collections;
using System.Collections.Generic;
using Easings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class TerminalEntry : MonoBehaviour {
    public TextMeshProUGUI text;
    public AudioSource audioSource;
    public IEnumerator Write(Writeln input, AudioClip[] audioClips) {
        // text.text = input;
        text.text = "";
        text.color = input.color;
        IEnumerator blinker = input.flash ? Toolbox.CoroutineFunc(() => { StartCoroutine(Toolbox.BlinkColor(text, input.color)); }) : Toolbox.CoroutineFunc(() => { });
        return Toolbox.ChainCoroutines(
            Toolbox.CoroutineFunc(() => {
                if (input.playerType) {
                    Toolbox.RandomizeOneShot(audioSource, audioClips);
                }
            }),
            Toolbox.TypeText(text, input.prefix, input.content, typedInput: input.playerType),
            Toolbox.CoroutineFunc(() => {
                if (input.playerType) {
                    audioSource.Stop();
                }
            }),
            blinker
        );
    }
}
