using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecurityIndexedValue<T> {
    public T laxValue;
    public T commercialValue;
    public T hardenedValue;

    public T get(LevelTemplate.SecurityLevel level) => level switch {
        LevelTemplate.SecurityLevel.lax => laxValue,
        LevelTemplate.SecurityLevel.commercial => commercialValue,
        LevelTemplate.SecurityLevel.hardened => hardenedValue,
        _ => laxValue
    };
}

[System.Serializable]
public class SecurityIndexedLoHiValue : SecurityIndexedValue<LoHi> {
}

[System.Serializable]
public class SecurityIndexedFloatValue : SecurityIndexedValue<float> {
}

[System.Serializable]
public class DoorLockRandomizerTemplate {
    public SecurityIndexedFloatValue setbackProbability;
    public SecurityIndexedLoHiValue progressStages;
    public float getSetbackProb(LevelTemplate.SecurityLevel security) => Random.Range(0f, setbackProbability.get(security));
    public int getProgressStages(LevelTemplate.SecurityLevel security) => (int)Random.Range(progressStages.get(security).low, progressStages.get(security).high);
}

[CreateAssetMenu(menuName = "ScriptableObjects/DoorRandomizerTemplate")]
public class DoorRandomizerTemplate : ScriptableObject {
    public string doorName;
    public Color color;

    public SecurityIndexedFloatValue latchesEnabled;
    public SecurityIndexedFloatValue latchesVulnerable;
    public SecurityIndexedFloatValue latchGuardEnabled;
    public SecurityIndexedFloatValue latchGuardScrews;
    public SecurityIndexedFloatValue deadboltEnabled;
    public SecurityIndexedFloatValue autoClose;

    public DoorLockRandomizerTemplate knobLock;

    public bool getLatchesEnabled(LevelTemplate.SecurityLevel security) => Random.Range(0f, 1f) < latchesEnabled.get(security);
    public bool getLatchesVulnerable(LevelTemplate.SecurityLevel security) => Random.Range(0f, 1f) < latchesVulnerable.get(security);
    public bool getLatchGuardEnabled(LevelTemplate.SecurityLevel security) => Random.Range(0f, 1f) < latchGuardEnabled.get(security);
    public bool getLatchGuardScrews(LevelTemplate.SecurityLevel security) => Random.Range(0f, 1f) < latchGuardScrews.get(security);
    public bool getDeadboltEnabled(LevelTemplate.SecurityLevel security) => Random.Range(0f, 1f) < deadboltEnabled.get(security);
    public bool getAutoClose(LevelTemplate.SecurityLevel security) => Random.Range(0f, 1f) < autoClose.get(security);

}