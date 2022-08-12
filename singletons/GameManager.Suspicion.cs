using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
public partial class GameManager : Singleton<GameManager> {
    public static Action OnSuspicionChange;
    public Dictionary<String, SuspicionRecord> suspicionRecords = new Dictionary<string, SuspicionRecord>();

    public void AddSuspicionRecord(SuspicionRecord record) {
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
        gameData.levelData.sensitivityLevel;

    public Reaction GetSuspicionReaction() {
        Suspiciousness totalSuspicion = GetTotalSuspicion();
        switch (GetCurrentSensitivity()) {
            default:
            case SensitivityLevel.semiprivateProperty:
            case SensitivityLevel.publicProperty:
                if (totalSuspicion < Suspiciousness.aggressive) {
                    return Reaction.ignore;
                } else return Reaction.attack;
            case SensitivityLevel.privateProperty:
                if (totalSuspicion == Suspiciousness.normal) {
                    return Reaction.ignore;
                } else if (totalSuspicion == Suspiciousness.suspicious) {
                    return Reaction.investigate;
                } else {
                    return Reaction.attack;
                }
            case SensitivityLevel.restrictedProperty:
                if (totalSuspicion == Suspiciousness.normal) {
                    return Reaction.investigate;
                } else {
                    return Reaction.attack;
                }
        }
    }

    // TODO: handle lifetimes

    // TODO: interactor
}