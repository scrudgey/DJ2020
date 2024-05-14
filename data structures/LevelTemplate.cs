using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]

[CreateAssetMenu(menuName = "ScriptableObjects/LevelTemplate")]
public class LevelTemplate : ScriptableObject {
    public enum SecurityLevel { lax, commercial, hardened }
    public enum StrikeTeamCompletionThreshold { timer, clear }
    public enum StrikeTeamResponseBehavior { clear, investigate }
    public enum StrikeTeamCompletionBehavior { patrol, leave }
    public enum GraphVisibilityDefault { all, none, partial }

    [Header("level and scene")]
    public string levelName;
    public string readableMissionName;
    public SceneData initialScene;
    // public int mapviewInitialFloor = 1;
    // public List<float> floorHeights;
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
    public List<LevelTemplate> unlockLevels;

    [Header("gameplay parameters")]
    public SensitivityLevel sensitivityLevel;
    public GraphVisibilityDefault cyberGraphVisibilityDefault;
    public GraphVisibilityDefault alarmGraphVisibiltyDefault;
    public GraphVisibilityDefault powerGraphVisibilityDefault;
    public int maxInitialNPC;
    public int minNPC;
    public AudioClip alarmAudioClip;
    public float npcSpawnInterval = 2f;
    [Header("strike team")]
    public NPCTemplate strikeTeamTemplate;
    public float strikeTeamResponseTime;
    public float strikeTeamSpawnInterval = 0.5f;
    public int strikeTeamMaxSize = 3;
    public int maxNPC;
    public StrikeTeamResponseBehavior strikeTeamBehavior;
    public StrikeTeamCompletionThreshold strikeCompletionThreshold;
    public StrikeTeamCompletionBehavior strikeTeamCompletion;




    public static LevelTemplate LoadAsInstance(string name) {
        return ScriptableObject.Instantiate(Resources.Load($"data/missions/{name}/{name}") as LevelTemplate);
    }
    public static LevelTemplate LoadResource(string name) {
        string path = $"data/missions/{name}/{name}";
        Debug.Log($"template path: {path}");
        return Resources.Load<LevelTemplate>(path) as LevelTemplate;
    }



    public List<Objective> AllObjectives() => objectives.Concat(bonusObjectives).ToList();
}