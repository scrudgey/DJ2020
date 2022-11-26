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
        // resourcePaths = populator.entries
        //     .GroupBy(p => p.key)
        //     .Select(g => g.First())
        //     .ToDictionary(x => x.key, x => x.value);

        Debug.Log($"resource reference size: {resourcePaths.Count}");
        // Debug.Log(GetPath(clip));
        // Debug.Log(GetPath(sprite));
        // Debug.Log(GetPath(gun));
    }
    // void Start() {
    //     resourcePaths = new Dictionary<UnityEngine.Object, string>();
    //     DirectoryInfo levelDirectoryPath = new DirectoryInfo(Application.dataPath);
    //     FileInfo[] fileInfo = levelDirectoryPath.GetFiles("*.*", SearchOption.AllDirectories);
    //     foreach (FileInfo file in fileInfo) {
    //         if (file.FullName.Contains(RESOURCES) && file.Extension != ".meta") {
    //             string path = file.FullName;
    //             if (path.Contains(".DS_Store"))
    //                 continue;
    //             if (path.Contains(".git"))
    //                 continue;
    //             if (path.Contains("/Scripts/"))
    //                 continue;
    //             int startIndex = path.IndexOf(RESOURCES);
    //             string resourcePath = path.Substring(startIndex + RESOURCES.Length + 1, path.Length - startIndex - RESOURCES.Length - 1);
    //             string finalpath = resourcePath.Substring(0, resourcePath.Length - file.Extension.Length);
    //             Debug.Log($"resource reference load: {finalpath}");
    //             UnityEngine.Object obj = Resources.Load(finalpath);
    //             if (obj != null) {
    //                 resourcePaths[obj] = finalpath;
    //             } else {
    //                 Debug.LogError("Failed to load resource");
    //             }
    //         }
    //     }


    // }

    public string GetPath(UnityEngine.Object obj) {
        if (obj is Sprite) {
            Sprite s = (Sprite)obj;
            if (resourcePaths.ContainsKey(s)) {
                Debug.Log($"sprite found: {s}");
            }
            if (resourcePaths.ContainsKey(s.texture)) {
                Debug.Log($"found sprite texture: {s.texture}");
            } else {
                foreach (UnityEngine.Object key in resourcePaths.Keys) {
                    Debug.Log(key);
                }
            }
        }
        return obj switch {
            Sprite s => resourcePaths[s.texture],
            _ => resourcePaths[obj]
        };
    }

}
