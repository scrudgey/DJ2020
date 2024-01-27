using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Newtonsoft.Json;
using UnityEngine;
[System.Serializable]
public class LevelState : PerkIdConstants {
    [JsonConverter(typeof(ScriptableObjectJsonConverter<LevelTemplate>))]
    public LevelTemplate template;
    public LevelPlan plan;
    public LevelDelta delta;

    // this is weird: and temporary state.
    public Dictionary<string, ObjectiveLootSpawnpoint> spawnPoints;

    public static LevelState Instantiate(LevelTemplate template, LevelPlan plan, PlayerState playerState) {
        int numberDialogueCards = playerState.PerkIsActivated(PerkIdConstants.PERKID_SPEECH_3CARD) ? 3 : 2;

        CyberGraph cyberGraph = CyberGraph.LoadAll(template.levelName);
        PowerGraph powerGraph = PowerGraph.LoadAll(template.levelName);
        AlarmGraph alarmGraph = AlarmGraph.LoadAll(template.levelName);

        cyberGraph.ApplyVisibilityDefault(template.cyberGraphVisibilityDefault);
        powerGraph.ApplyVisibilityDefault(template.powerGraphVisibilityDefault);
        alarmGraph.ApplyVisibilityDefault(template.alarmGraphVisibiltyDefault);

        cyberGraph.Apply(plan);
        powerGraph.Apply(plan);
        alarmGraph.Apply(plan);

        // cyberGraph.InfillRandomData();/


        return new LevelState {
            template = template,
            plan = plan,
            delta = LevelDelta.Empty() with {
                phase = LevelDelta.MissionPhase.action,
                powerGraph = powerGraph,
                cyberGraph = cyberGraph,
                alarmGraph = alarmGraph,
                disguise = plan.startWithDisguise(),
                dialogueCards = Enumerable.Range(0, numberDialogueCards).Select((i) => playerState.NewDialogueCard()).ToList(),
                // objectivesState = template.objectives.Concat(template.bonusObjectives)
                // .ToDictionary(t => t, t => ObjectiveStatus.inProgress),
                // levelAcquiredPaydata = new List<PayData> { Resources.Load("data/paydata/personnel_data") as PayData }
                levelAcquiredPaydata = new List<PayData> { }
            }
        };
    }

    static List<DialogueCard> initializeDialogueCards() {
        return new List<DialogueCard>(){
            new DialogueCard(){
                type = DialogueTacticType.lie,
                baseValue = 10
            },
            new DialogueCard(){
                type = DialogueTacticType.lie,
                baseValue = 8
            },
            new DialogueCard(){
                type = DialogueTacticType.deny,
                baseValue = 6
            }
        };
    }

    public static LevelState Instantiate(LevelTemplate template, LevelDelta delta) => new LevelState {
        template = template,
        delta = delta
    };

    public bool anyAlarmTerminalActivated() {
        if (delta == null || delta.alarmGraph == null) {
            return false;
        } else {
            return delta.alarmGraph.anyAlarmTerminalActivated();
        }
    }

    public static string LevelDataPath(string levelName, bool includeDataPath = true) {
        string path = includeDataPath ? Path.Combine(Application.dataPath, "Resources", "data", "missions", levelName) :
                                        Path.Combine("data", "missions", levelName);
        // if (!Directory.Exists(path)) {
        //     Directory.CreateDirectory(path);
        // }
        return path;
    }

    public bool PlayerHasID() => plan.activeTactics.Any(tactic => tactic is TacticFakeID);

    public int NumberPreviousTacticType(DialogueTacticType type) {
        // if (delta.lastTactics.Count == 0) {
        //     return 0;
        // } else if (delta.lastTactics.Peek() == type) {
        //     return delta.lastTactics.Count;
        // } else return 0;
        return delta.lastTactics.Where(tactic => tactic == type).Count();
    }
}