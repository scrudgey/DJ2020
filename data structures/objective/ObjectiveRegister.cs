using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]

[CreateAssetMenu(menuName = "ScriptableObjects/Objectives/ObjectiveRegister")]
public class ObjectiveRegister : Objective {
    public string targetObject;
    bool objectHasBeenUsed;
    ObjectiveDelta delta;
    public override ObjectiveDelta ToDelta(LevelState state) {
        string targetIdn = state.SetLocationOfObjective(this);

        ObjectiveLootSpawnpoint spawnpoint = state.spawnPoints[targetIdn];

        delta = new ObjectiveDelta(this, () => spawnpoint.transform.position, targetIdn);
        if (state.plan.objectiveLocations.ContainsKey(name)) {
            delta.visibility = Visibility.known;
        }

        CashRegister register = spawnpoint.GetComponentInChildren<CashRegister>();
        register.OnRegisterOpened += HandleRegisterOpened;
        return delta;
    }

    void HandleRegisterOpened(List<Interactive> credsticks) {
        foreach (Interactive interactive in credsticks) {
            interactive.OnUsed += HandleCredstickCollected;
        }
    }

    void HandleCredstickCollected() {
        delta.progressInt += 1;
        if (delta.progressInt >= 3) {
            delta.status = ObjectiveStatus.complete;
        }
    }
}