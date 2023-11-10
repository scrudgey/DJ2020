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

    string pluralTactic(DialogueTacticType type) => type switch {
        DialogueTacticType.bluff => "bluffs",
        DialogueTacticType.challenge => "challenges",
        DialogueTacticType.deny => "denials",
        DialogueTacticType.escape => "escapes",
        DialogueTacticType.item => "items",
        DialogueTacticType.lie => "lies",
        DialogueTacticType.none => "nothings",
        DialogueTacticType.redirect => "redirections",
    };

    public Dictionary<string, int> getStatusEffects(DialogueInput input) {
        Dictionary<string, int> effects = new Dictionary<string, int>();

        effects.Add($"speech skill {input.playerSpeechSkill}", 5 * input.playerSpeechSkill);

        if (input.playerInDisguise) {
            effects.Add($"in disguise", -10);
        }

        switch (input.playerSuspiciousness) {
            case Suspiciousness.normal:
                break;
            case Suspiciousness.suspicious:
                effects.Add($"suspicious appearance", +7);
                break;
            case Suspiciousness.aggressive:
                effects.Add($"aggressive appearance", +15);
                break;
        }

        int previousTacticPenalty = GameManager.I.gameData.levelState.NumberPreviousTacticType(type);
        if (previousTacticPenalty > 0) {
            int magnitude = (int)((float)baseValue * ((float)previousTacticPenalty / (float)(previousTacticPenalty + 1)));
            string plural = pluralTactic(type);
            effects.Add($"used {previousTacticPenalty} {plural}", magnitude);
        }

        return effects;
    }
}