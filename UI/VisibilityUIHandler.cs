using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
public class VisibilityUIHandler : IBinder<LightLevelProbe> {
    public TextMeshProUGUI textMesh;
    // static readonly Dictionary<float, string> VALUES = new Dictionary<float, string>{
    //     {0, "<mark=#262629FF>_<font=\"8bo\"><color=#ffffff>-----</font></color>_</mark>" },
    //     {1, "<mark=#373D49FF>_<font=\"8bo\"><color=#ffffff>+----</font></color>_</mark>" },
    //     {2, "<mark=#4E725CFF>_<font=\"8bo\"><color=#ffffff>++---</font></color>_</mark>" },
    //     {3, "<mark=#56B544FF>_<font=\"8bo\"><color=#ffffff>+++--</font></color>_</mark>" },
    //     {4, "<mark=#ADC32AFF>_<font=\"8bo\"><color=#ffffff>++++-</font></color>_</mark>" },
    //     {5, "<mark=#CC5858FF>_<font=\"8bo\"><color=#ffffff>+++++</font></color>_</mark>" },
    // };

    static readonly Dictionary<float, string> VALUES = new Dictionary<float, string>{
        {0, "<mark=#2f3542>_<font=\"8bo\"><color=#ffffff>-----</font></color>_</mark>" },
        {1, "<mark=#2ed573>_<font=\"8bo\"><color=#ffffff>+----</font></color>_</mark>" },
        {2, "<mark=#7bed9f>_<font=\"8bo\"><color=#ffffff>++---</font></color>_</mark>" },
        {3, "<mark=#ffa502>_<font=\"8bo\"><color=#ffffff>+++--</font></color>_</mark>" },
        {4, "<mark=#eccc68>_<font=\"8bo\"><color=#ffffff>++++-</font></color>_</mark>" },
        {5, "<mark=#ff4757>_<font=\"8bo\"><color=#ffffff>+++++</font></color>_</mark>" },
    };
    float highestVis = float.MinValue;
    float lowestVis = float.MaxValue;
    override public void HandleValueChanged(LightLevelProbe t) {
        int key = t.GetDiscreteLightLevel();
        textMesh.text = VALUES[key];
    }
}
