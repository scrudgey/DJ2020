using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
public class VisibilityUIHandler : IBinder<LightLevelProbe> {
    public TextMeshProUGUI textMesh;
    // static readonly string HIGH_VIS_STRING = "<mark=#FF28287C>_<font=\"8bo\"><color=#ffffff>High Vis</font></color>_</mark>";
    // static readonly Dictionary<float, string> VALUES = new Dictionary<float, string>{
    //     {0f, "<mark=#262629FF>_<font=\"8bo\"><color=#ffffff>-----</font></color>_</mark>" },
    //     {20f, "<mark=#373D49FF>_<font=\"8bo\"><color=#ffffff>+----</font></color>_</mark>" },
    //     {40f, "<mark=#4E725CFF>_<font=\"8bo\"><color=#ffffff>++---</font></color>_</mark>" },
    //     {60f, "<mark=#56B544FF>_<font=\"8bo\"><color=#ffffff>+++--</font></color>_</mark>" },
    //     {80f, "<mark=#ADC32AFF>_<font=\"8bo\"><color=#ffffff>++++-</font></color>_</mark>" },
    //     {100f, "<mark=#28287CFF>_<font=\"8bo\"><color=#ffffff>+++++</font></color>_</mark>" },
    // };

    static readonly Dictionary<float, string> VALUES = new Dictionary<float, string>{
        {0, "<mark=#262629FF>_<font=\"8bo\"><color=#ffffff>-----</font></color>_</mark>" },
        {1, "<mark=#373D49FF>_<font=\"8bo\"><color=#ffffff>+----</font></color>_</mark>" },
        {2, "<mark=#4E725CFF>_<font=\"8bo\"><color=#ffffff>++---</font></color>_</mark>" },
        {3, "<mark=#56B544FF>_<font=\"8bo\"><color=#ffffff>+++--</font></color>_</mark>" },
        {4, "<mark=#ADC32AFF>_<font=\"8bo\"><color=#ffffff>++++-</font></color>_</mark>" },
        {5, "<mark=#CC5858FF>_<font=\"8bo\"><color=#ffffff>+++++</font></color>_</mark>" },
    };
    float highestVis = float.MinValue;
    float lowestVis = float.MaxValue;
    override public void HandleValueChanged(LightLevelProbe t) {
        int key = t.GetDiscreteLightLevel();
        textMesh.text = VALUES[key];
    }
}
