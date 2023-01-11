using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Items {
    [CreateAssetMenu(menuName = "ScriptableObjects/Items/RocketLauncher")]
    public class RocketLauncherData : ItemData {
        public GameObject rocketPrefab;
        public AudioClip[] deploySound;
        public AudioClip[] shootSound;
        public Octet<Sprite[]> spritesheet;
        public float rocketVelocity;
    }
}