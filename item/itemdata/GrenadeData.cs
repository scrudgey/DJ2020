using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Items {
    [CreateAssetMenu(menuName = "ScriptableObjects/Items/Grenade")]
    public class GrenadeData : ItemData {
        public GameObject grenadePrefab;
        public AudioClip[] deploySound;
        public AudioClip[] throwSound;
    }
}