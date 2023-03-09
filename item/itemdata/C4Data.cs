using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
namespace Items {
    [CreateAssetMenu(menuName = "ScriptableObjects/Items/C4")]
    public class C4Data : ItemData {
        [JsonConverter(typeof(ScriptableObjectJsonConverter<GameObject>))]
        public GameObject prefab;
        [JsonConverter(typeof(ScriptableObjectJsonConverter<AudioClip>))]
        public AudioClip deploySound;
        [JsonConverter(typeof(ScriptableObjectJsonConverter<ExplosionData>))]
        public ExplosionData explosionData;
    }
}