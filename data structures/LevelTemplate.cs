using System.Collections.Generic;
using UnityEngine;

[System.Serializable]

[CreateAssetMenu(menuName = "ScriptableObjects/LevelTemplate")]
public class LevelTemplate : ScriptableObject {
    public string levelName;
    public string sceneName;
    public string sceneDescriptor;
    public SensitivityLevel sensitivityLevel;
    public AudioClip alarmAudioClip;
    public float strikeTeamResponseTime;
    public NPCTemplate strikeTeamTemplate;
    public List<Objective> objectives;
    public List<Tactic> availableTactics;

    public Faction faction;

    public int creditReward;
    public TextAsset prompt;
    public TextAsset email;
    public static LevelTemplate LoadAsInstance(string name) {
        return ScriptableObject.Instantiate(Resources.Load($"data/levels/{name}/{name}") as LevelTemplate);
    }
}