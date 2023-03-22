using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SocialGroup : MonoBehaviour {

    public HashSet<WorldNPCAI> members = new HashSet<WorldNPCAI>();
    public WorldNPCAI currentSpeaker;

    public bool ShouldISpeak(WorldNPCAI ai) {
        if (currentSpeaker == null && members.Count > 1) {
            return Random.Range(0f, 1f) < 0.02f;
        } else {
            return false;
        }
    }
    public void DeregisterSpeaker() {
        currentSpeaker = null;
    }
    public void RegisterSpeaker(WorldNPCAI ai) {
        currentSpeaker = ai;
    }

    public void AddMember(WorldNPCAI member) {
        members.Add(member);
    }

    public void RemoveMember(WorldNPCAI member) {
        if (currentSpeaker == member) {
            DeregisterSpeaker();
        }
        members.Remove(member);
        if (members.Count == 0) {
            Destroy(gameObject);
        }
    }
}
