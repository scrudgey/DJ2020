using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
public class ObjectListJsonConverter<T> : JsonConverter<List<T>> where T : UnityEngine.Object {
    public static class Constants {
        public static readonly string LIST = "list";
    }
    public override List<T> ReadJson(JsonReader reader, Type objectType, List<T> existingValue, bool hasExistingValue, JsonSerializer serializer) {
        if (reader.TokenType == JsonToken.Null) return null;
        JObject jo = JObject.Load(reader);
        // string path = (string)jo[Constants.LIST];
        // var result = Resources.Load<T>(path) as T;
        // Debug.Log($"deserializing asset : {path} -> {result}");
        JArray etiquetteJArray = (JArray)jo[Constants.LIST];
        List<T> etiquetteList = new List<T>();
        for (int i = 0; i < etiquetteJArray.Count; i++) {
            // etiquetteList.Add((T)etiquetteJArray[i]);
            etiquetteList.Add(ScriptableObjectJsonConverter<T>.DoReadJson(reader, objectType, null, false, serializer));
        }
        // result.etiquettes = etiquetteList.ToArray();
        return etiquetteList;
    }
    public override void WriteJson(JsonWriter writer, List<T> value, JsonSerializer serializer) {
        writer.WriteStartObject();

        writer.WritePropertyName(Constants.LIST);
        writer.WriteStartArray();
        for (int i = 0; i < value.Count; i++) {
            // writer.WriteValue((int)value[i]);
            ScriptableObjectJsonConverter<T>.DoWriteJson(writer, value[i], serializer);
        }
        writer.WriteEndArray();
        // if (value != null) {
        //     string relativePath = Toolbox.AssetRelativePath(value);
        //     // Debug.Log($"serializing asset :  {relativePath} -> {value}");
        //     writer.WritePropertyName(Constants.PATH);
        //     writer.WriteValue(relativePath);
        // }
        writer.WriteEndObject();
    }
}