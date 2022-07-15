using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum HitState { none, stun, unconscious, dead };
public class MessageHitStun : Message {
    public HitState hitState;
}
