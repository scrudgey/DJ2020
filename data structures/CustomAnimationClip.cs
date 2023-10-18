using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;

[System.Serializable]
public class CustomAnimationClip {
    public AnimationEvent[] events;
    public float length;
    public WrapMode wrapMode;

    // required for serialization
    public CustomAnimationClip() { }
    public CustomAnimationClip(AnimationEvent[] events, AnimationClip clip) {
        this.events = events.OrderBy(e => e.time).ToArray();
        this.length = clip.length;
        // Debug.Log($"animation clip length: {clip.length} custom length: {this.length}");
        this.wrapMode = clip.wrapMode;
    }

    public static CustomAnimationClip Load(string animationName) {
        string path = $"data/animation/{animationName}";
        TextAsset textAsset = Resources.Load<TextAsset>(path);
        return Load(textAsset);
    }

    public static CustomAnimationClip Load(TextAsset animation) {
        XmlSerializer serializer = new XmlSerializer(typeof(CustomAnimationClip));
        if (animation != null) {
            using (var reader = new System.IO.StringReader(animation.text)) {
                return serializer.Deserialize(reader) as CustomAnimationClip;
            }
        } else {
            Debug.LogError($"custom animation not readable: {animation}");
            return null;
        }
    }

    public void Write(string animationName) {
        string path = filePath(animationName);
        XmlSerializer serializer = new XmlSerializer(typeof(CustomAnimationClip));
        // string path = FilePath(levelName, sceneName);
        using (FileStream sceneStream = File.Create(path)) {
            serializer.Serialize(sceneStream, this);
        }
    }

    static string filePath(string animationName) {
        return Path.Combine(Application.dataPath, "Resources", "data", "animation", $"{animationName}.xml");
        // return $"data/animation/{animationName}";
    }
}
