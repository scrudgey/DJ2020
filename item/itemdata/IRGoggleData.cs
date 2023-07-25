using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Items {
    [CreateAssetMenu(menuName = "ScriptableObjects/Items/IRGoggles")]
    public class IRGoggleData : ItemTemplate {
        public AudioClip[] wearSounds;
    }
}