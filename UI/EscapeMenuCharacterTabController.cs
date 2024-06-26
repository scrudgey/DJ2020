using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class EscapeMenuCharacterTabController : MonoBehaviour {
    public CharacterView characterView;
    public TextMeshProUGUI skillPointsText;
    public TextMeshProUGUI playerLevel;
    public TextMeshProUGUI playerName;

    public void Initialize(GameData data) {
        if (data.phase == GamePhase.world) {
            characterView.Initialize(data, null);
        } else {
            characterView.currentWeaponState = new WeaponState();
            if (GameManager.I.playerGunHandler.gunInstance == null) {
                if (GameManager.I.playerMeleeHandler.meleeWeapon != null) {
                    characterView.currentWeaponState.type = WeaponType.melee;
                    characterView.currentWeaponState.meleeWeapon = GameManager.I.playerMeleeHandler.meleeWeapon;
                } else {
                    characterView.currentWeaponState.type = WeaponType.none;
                }
            } else if (GameManager.I.playerGunHandler.gunInstance != null) {
                characterView.currentWeaponState.type = WeaponType.gun;
                characterView.currentWeaponState.gunInstance = GameManager.I.playerGunHandler.gunInstance;
            }
            characterView.Initialize(data, data.levelState.plan);
        }
        PlayerState state = data.playerState;
        skillPointsText.text = $"skill points: {state.skillpoints}";
        playerLevel.text = $"level: {state.PlayerLevel()}\n\nbody: {state.bodySkillPoints}\nguns: {state.gunSkillPoints}\nhack: {state.hackSkillPoints}\nspeech: {state.speechSkillPoints}";
        playerName.text = data.filename;
    }

    public void PerkButtonCallback() {
        GameManager.I.ShowPerkMenu();
    }
}
