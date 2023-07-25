using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Items {
    [CreateAssetMenu(menuName = "ScriptableObjects/Items/Grenade")]
    public class GrenadeData : ItemTemplate {
        public GameObject grenadePrefab;
        public AudioClip[] deploySound;
        public AudioClip[] throwSound;
    }
}