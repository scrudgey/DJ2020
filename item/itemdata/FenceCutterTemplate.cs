using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Items {
    [CreateAssetMenu(menuName = "ScriptableObjects/Items/FenceCutters")]
    public class FenceCutterTemplate : ItemTemplate {
        public AudioClip[] deploySound;
        public AudioClip[] snipSound;
    }
}