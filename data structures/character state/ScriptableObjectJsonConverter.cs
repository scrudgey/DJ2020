using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
public class ScriptableObjectJsonConverter<T> : JsonConverter<T> where T : UnityEngine.Object {
    public static class Constants {
        public static readonly string PATH = "path";
    }
    public override T ReadJson(JsonReader reader, Type objectType, T existingValue, bool hasExistingValue, JsonSerializer serializer) {
        return DoReadJson(reader, objectType, existingValue, hasExistingValue, serializer);
    }
    public override void WriteJson(JsonWriter writer, T value, JsonSerializer serializer) {
        DoWriteJson(writer, value, serializer);
    }

    public static T DoReadJson(JsonReader reader, Type objectType, T existingValue, bool hasExistingValue, JsonSerializer serializer) {
        if (reader.TokenType == JsonToken.Null) return null;
        JObject jo = JObject.Load(reader);
        string path = (string)jo[Constants.PATH];
        var result = Resources.Load<T>(path) as T;
        // Debug.Log($"deserializing asset : {path} -> {result}");
        return result;
    }

    public static void DoWriteJson(JsonWriter writer, T value, JsonSerializer serializer) {
        writer.WriteStartObject();
        if (value != null) {
            string relativePath = Toolbox.AssetRelativePath(value);
            // Debug.Log($"serializing asset :  {relativePath} -> {value}");
            writer.WritePropertyName(Constants.PATH);
            writer.WriteValue(relativePath);
        }
        writer.WriteEndObject();
    }
}