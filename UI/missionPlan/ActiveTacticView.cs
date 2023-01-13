using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class ActiveTacticView : MonoBehaviour {
    public TextMeshProUGUI title;
    public void Initialize(Tactic tactic) {
        title.text = tactic.title;
    }
}
