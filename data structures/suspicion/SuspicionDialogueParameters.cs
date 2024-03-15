using System.Collections.Generic;
using System.Linq;
using Nimrod;
using UnityEngine;


[System.Serializable]
public class SuspicionDialogueParameters {
    public bool enabled = true;
    [TextArea(2, 20)]
    public string challenge;
    [TextArea(2, 20)]

    public string pastTenseChallenge;
    public DialogueTactic tacticLie;
    public DialogueTactic tacticDeny;
    public DialogueTactic tacticRedirect;
    public DialogueTactic tacticChallenge;
    public DialogueTactic tacticBluff;
    public DialogueTactic tacticItem;
}
