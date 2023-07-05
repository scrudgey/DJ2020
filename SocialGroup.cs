using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SocialGroup : MonoBehaviour {

    public HashSet<MonoBehaviour> members = new HashSet<MonoBehaviour>();
    public MonoBehaviour currentSpeaker;

    public bool ShouldISpeak(MonoBehaviour ai) {
        if (currentSpeaker == null && members.Count > 1) {
            return Random.Range(0f, 1f) < 0.02f;
        } else {
            return false;
        }
    }
    public void DeregisterSpeaker() {
        currentSpeaker = null;
    }
    public void RegisterSpeaker(MonoBehaviour ai) {
        currentSpeaker = ai;
    }

    public void AddMember(MonoBehaviour member) {
        members.Add(member);
    }

    public void RemoveMember(MonoBehaviour member) {
        if (currentSpeaker == member) {
            DeregisterSpeaker();
        }
        members.Remove(member);
        if (members.Count == 0) {
            Destroy(gameObject);
        }
    }
}
