using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PerkCategory { gun, hack, body, speech }
[CreateAssetMenu(menuName = "ScriptableObjects/Perk")]
public class Perk : ScriptableObject {
    public string readableName;
    public string perkId;
    public int stages = 1;
    public PerkCategory category;
    public Sprite icon;
    [TextArea(15, 20)]
    public string description;

    [Header("requirements")]
    public int skillLevelRequirement;
    public int playerLevelRequirement;
    public Perk[] requiredPerks;

    public bool CanBePurchased(PlayerState state) {
        bool canBePurchased = true;
        canBePurchased &= PlayerLevelRequirementMet(state);
        canBePurchased &= SkillLevelRequirementMet(state);
        foreach (Perk requiredPerk in requiredPerks) {
            canBePurchased &= PerkRequirementMet(state, requiredPerk);
        }
        return canBePurchased;
    }

    public bool PlayerLevelRequirementMet(PlayerState state) {
        int playerLevel = state.PlayerLevel();
        return playerLevel >= playerLevelRequirement;
    }

    public bool SkillLevelRequirementMet(PlayerState state) {
        int categoryLevel = category switch {
            PerkCategory.gun => state.gunSkillPoints,
            PerkCategory.hack => state.hackSkillPoints,
            PerkCategory.body => state.bodySkillPoints,
            PerkCategory.speech => state.speechSkillPoints
        };
        return categoryLevel >= skillLevelRequirement;
    }

    public bool PerkRequirementMet(PlayerState state, Perk requiredPerk) {
        return state.PerkIsFullyActivated(requiredPerk);
    }
    public bool IsMultiStagePerk() {
        return stages > 1;
    }

    public string PerkIdForLevel(int level) {
        return PerkIdForLevel(perkId, level);
    }

    public static string PerkIdForLevel(string perkId, int level) {
        return $"{perkId}_{level}";
    }

}