using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/PayData")]
public class PayData : ScriptableObject {
    public enum DataType { pay, personnel, password, location, objective }
    public DataType type;
    public string filename;
    public int value;
    [JsonConverter(typeof(ScriptableObjectJsonConverter<TextAsset>))]
    public TextAsset content;
}