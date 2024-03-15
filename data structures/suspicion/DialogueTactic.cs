using System.Collections.Generic;
using System.Linq;
using Nimrod;
using UnityEngine;


[System.Serializable]
public class DialogueTactic {
    public DialogueTacticType tacticType;
    [TextArea(1, 20)]
    public string content;
    [TextArea(1, 20)]
    public string successResponse;
    [TextArea(1, 20)]
    public string failResponse;
}
