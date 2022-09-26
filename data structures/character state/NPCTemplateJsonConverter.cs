using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
public class NPCTemplateJsonConverter : JsonConverter<NPCTemplate> {
    // required because NPCTemplate is a scriptableObject used as an instance, it cannot always be loaded from asset path
    public static class Constants {
        public static readonly string LEGSKIN = "legSkin";
        public static readonly string BODYSKIN = "bodySKin";
        public static readonly string PRIMARY_GUN = "primary";
        public static readonly string SECONDARY_GUN = "secondary";
        public static readonly string TERTIARY_GUN = "tertiary";
        public static readonly string HEALTH = "health";
        public static readonly string FULL_HEALTH = "fullhealth";
        public static readonly string HITSTATE = "hitstate";
    }

    public override NPCTemplate ReadJson(JsonReader reader, Type objectType, NPCTemplate existingValue, bool hasExistingValue, JsonSerializer serializer) {
        // Load the JSON for the Result into a JObject
        JObject jo = JObject.Load(reader);

        NPCTemplate result = ScriptableObject.CreateInstance<NPCTemplate>();

        result.legSkin = (string)jo[Constants.LEGSKIN];
        result.bodySkin = (string)jo[Constants.BODYSKIN];
        result.health = (float)jo[Constants.HEALTH];
        result.fullHealthAmount = (float)jo[Constants.FULL_HEALTH];
        result.hitState = (HitState)((int)jo[Constants.HITSTATE]);

        string gun1Path = (string)jo[Constants.PRIMARY_GUN];
        string gun2Path = (string)jo[Constants.SECONDARY_GUN];
        string gun3Path = (string)jo[Constants.TERTIARY_GUN];
        GunTemplate gun1 = Resources.Load(gun1Path) as GunTemplate;
        GunTemplate gun2 = Resources.Load(gun2Path) as GunTemplate;
        GunTemplate gun3 = Resources.Load(gun3Path) as GunTemplate;

        result.primaryGun = gun1;
        result.secondaryGun = gun2;
        result.tertiaryGun = gun3;

        return result;
    }
    public override void WriteJson(JsonWriter writer, NPCTemplate value, JsonSerializer serializer) {
        writer.WriteStartObject();
        if (value != null) {
            writer.WritePropertyName(Constants.LEGSKIN);
            writer.WriteValue(value.legSkin);
            writer.WritePropertyName(Constants.BODYSKIN);
            writer.WriteValue(value.bodySkin);

            writer.WritePropertyName(Constants.PRIMARY_GUN);
            writer.WriteValue(Toolbox.AssetRelativePath(value.primaryGun));
            writer.WritePropertyName(Constants.SECONDARY_GUN);
            writer.WriteValue(Toolbox.AssetRelativePath(value.secondaryGun));
            writer.WritePropertyName(Constants.TERTIARY_GUN);
            writer.WriteValue(Toolbox.AssetRelativePath(value.tertiaryGun));

            writer.WritePropertyName(Constants.HEALTH);
            writer.WriteValue(value.health);
            writer.WritePropertyName(Constants.FULL_HEALTH);
            writer.WriteValue(value.fullHealthAmount);
            writer.WritePropertyName(Constants.HITSTATE);
            writer.WriteValue((int)value.hitState);
        }
        writer.WriteEndObject();
    }
}