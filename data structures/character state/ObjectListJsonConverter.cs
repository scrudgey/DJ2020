using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
public class ObjectListJsonConverter<T> : JsonConverter<List<T>> where T : UnityEngine.Object {
    public static class Constants {
        public static readonly string LIST = "list";
        public static readonly string PATH = "path";

    }
    public override List<T> ReadJson(JsonReader reader, Type objectType, List<T> existingValue, bool hasExistingValue, JsonSerializer serializer) {
        if (reader.TokenType == JsonToken.Null) return null;
        List<T> etiquetteList = new List<T>();

        JObject jo = JObject.Load(reader);
        JArray etiquetteJArray = (JArray)jo[Constants.LIST];
        foreach (JObject item in etiquetteJArray) {
            // if (item.HasValues(Constants.PATH))
            try {

                string path = item.GetValue(Constants.PATH).ToString();
                T result = Resources.Load<T>(path) as T;
                etiquetteList.Add(result);
            }
            catch (Exception e) {
                // Debug.LogError($"error loading object list: {e}");
                // Debug.LogError($"typeof: {objectType}");
                etiquetteList.Add(null);
            }
        }

        return etiquetteList;
    }
    public override void WriteJson(JsonWriter writer, List<T> value, JsonSerializer serializer) {
        writer.WriteStartObject();
        writer.WritePropertyName(Constants.LIST);
        writer.WriteStartArray();
        for (int i = 0; i < value.Count; i++) {
            ScriptableObjectJsonConverter<T>.DoWriteJson(writer, value[i], serializer);
        }
        writer.WriteEndArray();
        writer.WriteEndObject();
    }
}