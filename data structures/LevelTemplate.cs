using System.Collections.Generic;
using UnityEngine;

[System.Serializable]

[CreateAssetMenu(menuName = "ScriptableObjects/LevelTemplate")]
public class LevelTemplate : ScriptableObject {
    public enum SecurityLevel { lax, commercial, hardened }

    [Header("level and scene")]
    public string levelName;
    public string sceneName;
    public string sceneDescriptor;
    public int mapviewInitialFloor = 1;
    public string tagline;
    public MusicTrack musicTrack;
    [Header("text")]
    public TextAsset proposalEmail;
    public TextAsset successEmail;
    [TextArea(15, 20)]
    public string shortDescription;
    public Alertness guardAlertness;
    public SecurityLevel securityLevel;
    [TextArea(15, 20)]
    public string securityDescription;
    [Header("mission")]
    public Faction faction;
    public int creditReward;
    public List<Objective> objectives;
    public List<Objective> bonusObjectives;
    public List<Tactic> availableTactics;

    [Header("gameplay parameters")]
    public SensitivityLevel sensitivityLevel;
    public AudioClip alarmAudioClip;
    public float strikeTeamResponseTime;
    public NPCTemplate strikeTeamTemplate;
    public int maxInitialNPC;
    public int maxNPC;

    public static LevelTemplate LoadAsInstance(string name) {
        return ScriptableObject.Instantiate(Resources.Load($"data/levels/{name}/{name}") as LevelTemplate);
    }
    public static LevelTemplate LoadResource(string name) {
        return Resources.Load($"data/levels/{name}/{name}") as LevelTemplate;
    }
}