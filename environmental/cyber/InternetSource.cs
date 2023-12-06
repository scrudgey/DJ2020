using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InternetSource : CyberComponent {
    public override void EnableSource() {
        base.EnableSource();
        GameManager.I.SetCyberNodeCompromised(this, true);
    }
    public override void ConfigureNode(CyberNode node) {
        node.compromised = true;
    }
}
