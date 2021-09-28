using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Items {
    [CreateAssetMenu(menuName = "ScriptableObjects/Items/C4")]
    public class C4Data : ItemData {
        public GameObject prefab;
        public AudioClip deploySound;
    }
}