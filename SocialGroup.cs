using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SocialGroup : MonoBehaviour {

    public HashSet<WorldNPCAI> members = new HashSet<WorldNPCAI>();

    public void AddMember(WorldNPCAI member) {
        members.Add(member);
    }

    public void RemoveMember(WorldNPCAI member) {
        members.Remove(member);
        if (members.Count == 0) {
            Destroy(gameObject);
        }
    }
}
