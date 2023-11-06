using System;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class DialogueCard {
    public DialogueTacticType type;
    public int baseValue;
    public int derivedValue(DialogueInput input) {
        Dictionary<string, int> effects = getStatusEffects(input);
        return baseValue + effects.Values.Sum();
    }

    public Dictionary<string, int> getStatusEffects(DialogueInput input) {
        Dictionary<string, int> effects = new Dictionary<string, int>();

        effects.Add($"speech skill {input.playerSpeechSkill}", 5 * input.playerSpeechSkill);

        if (input.playerInDisguise) {
            effects.Add($"in disguise", +10);
        }

        switch (input.playerSuspiciousness) {
            case Suspiciousness.normal:
                break;
            case Suspiciousness.suspicious:
                effects.Add($"suspicious appearance", -5);
                break;
            case Suspiciousness.aggressive:
                effects.Add($"aggressive appearance", -10);
                break;
        }

        int previousTacticPenalty = GameManager.I.gameData.levelState.NumberPreviousTacticType(type);
        if (previousTacticPenalty > 0) {
            int magnitude = (int)((float)baseValue * ((float)previousTacticPenalty / (float)(previousTacticPenalty + 1)));
            // 1/2 => 1 - 1/2 = 1/2
            // 1/3 => 1 - 1/3 = 2/3
            // 1/4 => 1 - 1/4 = 3/4
            effects.Add($"told {previousTacticPenalty} {type}s", -1 * magnitude);
        }

        return effects;
    }
}