using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HVACPath {
    public List<HVACElement> path;
}

public class HVACNetwork : MonoBehaviour {
    public List<HVACPath> paths;

    public Dictionary<HVACElement, HashSet<HVACElement>> neighbors;
    public AudioClip[] crawlSounds;

    public void Start() {
        neighbors = new Dictionary<HVACElement, HashSet<HVACElement>>();
        foreach (HVACPath hvacPath in paths) {
            for (int i = 1; i < hvacPath.path.Count; i++) {
                HVACElement a = hvacPath.path[i];
                HVACElement b = hvacPath.path[i - 1];
                AddEdge(a, b);
            }
        }
    }

    void AddEdge(HVACElement a, HVACElement b) {
        if (!neighbors.ContainsKey(a)) {
            neighbors[a] = new HashSet<HVACElement>();
        }
        if (!neighbors.ContainsKey(b)) {
            neighbors[b] = new HashSet<HVACElement>();
        }
        neighbors[a].Add(b);
        neighbors[b].Add(a);
    }

#if UNITY_EDITOR
    protected virtual void OnDrawGizmos() {
        Gizmos.color = Color.red;
        foreach (HVACPath path in paths) {
            if (path.path.Count <= 1) continue;
            for (int i = 1; i < path.path.Count; i++) {
                if (path.path[i] == null || path.path[i - 1] == null) continue;
                Vector3 point1 = path.path[i].crawlpoint.position;
                Vector3 point2 = path.path[i - 1].crawlpoint.position;
                Gizmos.DrawLine(point1 + Vector3.up / 2f, point2 + Vector3.up / 2f);
                // Debug.Log($"{point1} {point2}");
            }
        }
    }
#endif
}
