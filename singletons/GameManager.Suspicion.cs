using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
public partial class GameManager : Singleton<GameManager> {
    public static Action OnSuspicionChange;
    public Dictionary<String, SuspicionRecord> suspicionRecords = new Dictionary<string, SuspicionRecord>();
    void UpdateSuspicion() {
        List<SuspicionRecord> timedOutRecords = new List<SuspicionRecord>();
        foreach (SuspicionRecord record in suspicionRecords.Values) {
            if (!record.IsTimed())
                continue;
            record.Update(Time.deltaTime);
            if (record.lifetime <= 0) {
                timedOutRecords.Add(record);
            }
        }
        if (timedOutRecords.Count > 0) {
            foreach (SuspicionRecord record in timedOutRecords) {
                suspicionRecords.Remove(record.content);
            }
            OnSuspicionChange?.Invoke();
        }
    }
    public void AddSuspicionRecord(SuspicionRecord record) {
        if (record == null) return;
        if (record.suspiciousness == Suspiciousness.aggressive) {
            if (gameData.levelState.delta.disguise) {
                // TODO: display feedback iconography
                gameData.levelState.delta.disguise = false;
            }
        }
        suspicionRecords[record.content] = record;
        OnSuspicionChange?.Invoke();
    }

    public void RemoveSuspicionRecord(SuspicionRecord record) {
        if (suspicionRecords.ContainsKey(record.content)) {
            suspicionRecords.Remove(record.content);
            OnSuspicionChange?.Invoke();
        }
    }

    public Suspiciousness GetTotalSuspicion() {
        if (suspicionRecords.Count > 0) {
            return suspicionRecords.Values
                        .Select(record => record.suspiciousness)
                        .Aggregate((curMax, x) => ((int)x > (int)curMax) ? x : curMax);
        } else {
            return Suspiciousness.normal;
        }
    }

    public SensitivityLevel GetCurrentSensitivity() =>
        gameData.levelState.template.sensitivityLevel;

    public Reaction GetSuspicionReaction(Suspiciousness totalSuspicion, bool applyModifiers = true) {
        Reaction reaction = Reaction.ignore;
        switch (GetCurrentSensitivity()) {
            default:
            case SensitivityLevel.semiprivateProperty:
            case SensitivityLevel.publicProperty:
                if (totalSuspicion < Suspiciousness.aggressive) {
                    reaction = Reaction.ignore;
                } else reaction = Reaction.attack;
                break;
            case SensitivityLevel.privateProperty:
                if (totalSuspicion == Suspiciousness.normal) {
                    reaction = Reaction.ignore;
                } else if (totalSuspicion == Suspiciousness.suspicious) {
                    reaction = Reaction.investigate;
                } else {
                    reaction = Reaction.attack;
                }
                break;
            case SensitivityLevel.restrictedProperty:
                if (totalSuspicion == Suspiciousness.normal) {
                    reaction = Reaction.investigate;
                } else {
                    reaction = Reaction.attack;
                }
                break;
        }
        if (applyModifiers) {
            bool alarmActive = gameData.levelState.anyAlarmTerminalActivated();
            bool disguiseActive = gameData.levelState.delta.disguise;
            if (alarmActive && !disguiseActive) {
                if (reaction == Reaction.ignore) {
                    reaction = Reaction.investigate;
                } else if (reaction == Reaction.investigate) {
                    reaction = Reaction.attack;
                } else if (reaction == Reaction.attack) {
                    // trigger alarm again?
                    // GameManager.I.ActivateAlarm();
                }
            }
            if (disguiseActive && !alarmActive) {
                if (reaction == Reaction.investigate) {
                    reaction = Reaction.ignore;
                } else if (reaction == Reaction.attack) {
                    reaction = Reaction.investigate;
                }
            }
        }
        return reaction;
    }


    public void ActivateDisguise() {
        gameData.levelState.delta.disguise = true;
        OnSuspicionChange?.Invoke();
    }
    public void DeactivateDisguise() {
        gameData.levelState.delta.disguise = false;
        OnSuspicionChange?.Invoke();
    }
}