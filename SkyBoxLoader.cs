// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class SkyBoxLoader : MonoBehaviour {
//     public enum SkyBoxType { none, city }
//     public SkyBoxType type;
//     static readonly Dictionary<SkyBoxType, string> skyboxSceneNames = new Dictionary<SkyBoxType, string>{
//         {SkyBoxType.city, "cityskybox"}
//     };
//     void Start() {
//         if (type != SkyBoxType.none) {
//             GameManager.I.LoadSkyBox(skyboxSceneNames[type]);
//             Destroy(gameObject);
//         }
//     }
// }
