using System.Collections.Generic;
using UnityEngine;

[System.Serializable]

[CreateAssetMenu(menuName = "ScriptableObjects/LevelTemplate")]
public class LevelTemplate : ScriptableObject {
    public string levelName;
    public SensitivityLevel sensitivityLevel;
    public AudioClip alarmAudioClip;
    public int strikeTeamMaxSize;
    public float strikeTeamResponseTime;
    public NPCTemplate strikeTeamTemplate;
    public static LevelTemplate LoadAsInstance(string name) {
        return ScriptableObject.Instantiate(Resources.Load($"data/levels/{name}/levelTemplate") as LevelTemplate);
    }
}