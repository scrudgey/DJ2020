using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
public interface GraphJsonConverter {
    public static class Constants {
        public static readonly string LEVELNAME = "levelname";
    }
}


public class CyberGraphConverter : JsonConverter<CyberGraph>, GraphJsonConverter {
    public override CyberGraph ReadJson(JsonReader reader, Type objectType, CyberGraph existingValue, bool hasExistingValue, JsonSerializer serializer) {
        if (reader.TokenType == JsonToken.Null) return null;
        JObject jo = JObject.Load(reader);
        string levelname = (string)jo[GraphJsonConverter.Constants.LEVELNAME];
        return CyberGraph.LoadAll(levelname);
    }

    public override void WriteJson(JsonWriter writer, CyberGraph value, JsonSerializer serializer) {
        writer.WriteStartObject();
        if (value != null) {
            writer.WritePropertyName(GraphJsonConverter.Constants.LEVELNAME);
            writer.WriteValue(value.levelName);
        }
        writer.WriteEndObject();
    }
}

public class PowerGraphConverter : JsonConverter<PowerGraph>, GraphJsonConverter {
    public override PowerGraph ReadJson(JsonReader reader, Type objectType, PowerGraph existingValue, bool hasExistingValue, JsonSerializer serializer) {
        if (reader.TokenType == JsonToken.Null) return null;
        JObject jo = JObject.Load(reader);
        string levelname = (string)jo[GraphJsonConverter.Constants.LEVELNAME];
        return PowerGraph.LoadAll(levelname);
    }

    public override void WriteJson(JsonWriter writer, PowerGraph value, JsonSerializer serializer) {
        writer.WriteStartObject();
        if (value != null) {
            writer.WritePropertyName(GraphJsonConverter.Constants.LEVELNAME);
            writer.WriteValue(value.levelName);
        }
        writer.WriteEndObject();
    }
}

public class AlarmGraphConverter : JsonConverter<AlarmGraph>, GraphJsonConverter {
    public override AlarmGraph ReadJson(JsonReader reader, Type objectType, AlarmGraph existingValue, bool hasExistingValue, JsonSerializer serializer) {
        if (reader.TokenType == JsonToken.Null) return null;
        JObject jo = JObject.Load(reader);
        string levelname = (string)jo[GraphJsonConverter.Constants.LEVELNAME];
        return AlarmGraph.LoadAll(levelname);
    }

    public override void WriteJson(JsonWriter writer, AlarmGraph value, JsonSerializer serializer) {
        writer.WriteStartObject();
        if (value != null) {
            writer.WritePropertyName(GraphJsonConverter.Constants.LEVELNAME);
            writer.WriteValue(value.levelName);
        }
        writer.WriteEndObject();
    }
}