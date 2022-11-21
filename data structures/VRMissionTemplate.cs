using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
[System.Serializable]
public record VRMissionTemplate {
    public static readonly string DEFAULT_FILENAME = "defaultMission.json";

    public string filename;
    public string sceneName;
    public VRMissionType missionType;
    public SensitivityLevel sensitivityLevel;
    public int maxNumberNPCs;
    public float NPCspawnInterval = 5f;
    public int numberConcurrentNPCs;
    public PlayerTemplate playerState;
    public bool alarmHQEnabled;
    [JsonConverter(typeof(NPCTemplateJsonConverter))]
    public NPCTemplate npc1State;

    [JsonConverter(typeof(NPCTemplateJsonConverter))]
    public NPCTemplate npc2State;
    public int targetDataCount;
    public float timeLimit;

    public static VRMissionTemplate Default() => new VRMissionTemplate {
        filename = DEFAULT_FILENAME,
        sceneName = "VR_infiltration",
        missionType = VRMissionType.combat,
        sensitivityLevel = SensitivityLevel.restrictedProperty,
        maxNumberNPCs = 10,
        NPCspawnInterval = 5,
        numberConcurrentNPCs = 3,
        playerState = PlayerTemplate.Default(),
        npc1State = ScriptableObject.Instantiate(Resources.Load("data/npc/guard1") as NPCTemplate),
        npc2State = ScriptableObject.Instantiate(Resources.Load("data/npc/guard2") as NPCTemplate),
    };

    public static void SaveVRMissionTemplate(VRMissionTemplate template, string filename) {
        CreateScenarioFolderIfMissing(VRMissionRootDirectory());
        string path = VRMissionPath(filename);
        Debug.Log($"saving {path}...");
        try {
            using (FileStream fs = File.Open(path, FileMode.Create))
            using (StreamWriter sw = new StreamWriter(fs))
            using (JsonWriter jw = new JsonTextWriter(sw)) {
                JsonSerializer serializer = JsonSerializer.Create();
                serializer.Serialize(jw, template);
            }
            // Debug.Log($"wrote to {path}");
        }
        catch (Exception e) {
            Debug.LogError($"error writing to file: {path} {e}");
        }
    }
    public static VRMissionTemplate LoadVRMissionTemplate(string filename) {
        CreateScenarioFolderIfMissing(VRMissionRootDirectory());
        string path = VRMissionPath(filename);
        Debug.Log($"loading {path}...");
        try {
            // deserialize JSON directly from a file
            using (StreamReader file = File.OpenText(path)) {
                JsonSerializer serializer = new JsonSerializer();
                VRMissionTemplate template = (VRMissionTemplate)serializer.Deserialize(file, typeof(VRMissionTemplate));
                // Debug.Log($"successfully loaded VR mission template from {path}");
                return template;
            }
        }
        catch (Exception e) {
            Debug.LogError($"error reading VR template file: {path} {e}");
            return VRMissionTemplate.Default();
        }
    }
    // static public string VRMissionPath() => System.IO.Path.Join(Application.persistentDataPath, "vrmission.json");

    static public string VRMissionRootDirectory() => System.IO.Path.Join(Application.persistentDataPath, "vrMissions");
    static public string VRMissionPath(string filename) => System.IO.Path.Join(VRMissionRootDirectory(), filename);

    static public void CreateScenarioFolderIfMissing(string directoryPath) {
        try {
            if (!Directory.Exists(directoryPath)) {
                Directory.CreateDirectory(directoryPath);
                CreateDefaultVRTemplates(directoryPath);
            }
        }
        catch (IOException ex) {
            Debug.LogError(ex.Message);
        }
    }

    public static void CreateDefaultVRTemplates(string targetDirectoryPath) {
        UnityEngine.Object[] worlds = Resources.LoadAll("data/vrTemplates", typeof(TextAsset));

        // Make sure results aren't empty or null
        if (worlds == null || worlds.Length == 0) {
            Debug.LogError("no templates found!");
        }

        // Add each result to the worldList
        foreach (UnityEngine.Object world in worlds) {
            TextAsset w = (TextAsset)world;
            string filename = System.IO.Path.Join(targetDirectoryPath, w.name);
            using (StreamWriter writeFile = new StreamWriter(filename, false)) {
                foreach (string line in w.text.Split("\n")) {
                    writeFile.AutoFlush = true;
                    writeFile.WriteLine(line);
                }
            }
        }

        // Unload them - no longer needed
        // foreach (UnityEngine.Object world in worlds) {
        //     Resources.UnloadAsset(world);
        // }
    }

}