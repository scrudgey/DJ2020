using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
public class ResourceReference : Singleton<ResourceReference> {
    public static readonly string RESOURCES = "Resources";
    public ResourceReferencePopulator populator;
    public Dictionary<UnityEngine.Object, string> resourcePaths;
    public AudioClip clip;
    public Sprite sprite;
    public GunTemplate gun;
    void Awake() {
        resourcePaths = new Dictionary<UnityEngine.Object, string>();
        resourcePaths = populator.keys.Zip(populator.values, (key, value) => (key, value)).GroupBy(x => x.key).Select(g => g.First()).ToDictionary(x => x.key, x => x.value);
        Debug.Log($"resource reference size: {resourcePaths.Count}");
    }

    public string GetPath(UnityEngine.Object obj) {
        if (obj == null) {
            return "null";
        }
        return obj switch {
            Sprite s => resourcePaths[s.texture],
            _ => resourcePaths[obj]
        };
    }

}
