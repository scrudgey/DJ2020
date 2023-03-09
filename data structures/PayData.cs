using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/PayData")]
public class PayData : ScriptableObject {
    public string filename;
    public int value;
    public TextAsset content;
}