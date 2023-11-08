using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Newtonsoft.Json;
using UnityEngine;
[System.Serializable]
public class LevelState {
    [JsonConverter(typeof(ScriptableObjectJsonConverter<LevelTemplate>))]
    public LevelTemplate template;
    public LevelPlan plan;
    public LevelDelta delta;

    public static LevelState Instantiate(LevelTemplate template, LevelPlan plan, PlayerState playerState) => new LevelState {
        template = template,
        plan = plan,
        delta = LevelDelta.Empty() with {
            phase = LevelDelta.MissionPhase.action,
            powerGraph = PowerGraph.LoadAll(template.levelName),
            cyberGraph = CyberGraph.LoadAll(template.levelName),
            alarmGraph = AlarmGraph.LoadAll(template.levelName),
            disguise = plan.startWithDisguise(),
            dialogueCards = Enumerable.Range(0, 2).Select((i) => playerState.NewDialogueCard()).ToList(),
            objectivesState = template.objectives.Concat(template.bonusObjectives)
                .ToDictionary(t => t, t => ObjectiveStatus.inProgress)
        }
    };

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
        if (delta.lastTactics.Count == 0) {
            return 0;
        } else if (delta.lastTactics.Peek() == type) {
            return delta.lastTactics.Count;
        } else return 0;
    }
}