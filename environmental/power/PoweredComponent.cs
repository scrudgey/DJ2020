using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum PowerNodeIcon { normal, power, mains }
public class PoweredComponent : MonoBehaviour {
    public string idn;
    public string nodeTitle;
    public PowerNodeIcon icon;
    private bool _power;
    public bool power {
        get { return _power; }
        set {
            _power = value;
            if (_power) {
                OnPowerOn();
            } else { OnPowerOff(); }
        }
    }
    public PoweredComponent[] edges;

    public Vector3 NodePosition() {
        Transform nodePositioner = transform.Find("node");
        // nodePositioner? nodePositioner.p
        if (nodePositioner) {
            return nodePositioner.position;
        } else return transform.position;
    }

    virtual protected void OnPowerOn() { }
    virtual protected void OnPowerOff() { }


#if UNITY_EDITOR
    private void OnDrawGizmos() {
        // string customName = "Relic\\" + relicType.ToString() + ".png";
        // Gizmos.DrawIcon(transform.position, customName, true);
        foreach (PoweredComponent other in edges) {
            Gizmos.DrawLine(NodePosition(), other.NodePosition());
        }
    }
#endif
}
