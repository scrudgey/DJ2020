using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]

[CreateAssetMenu(menuName = "ScriptableObjects/Objectives/ObjectiveUseObject")]
public class ObjectiveUseObject : Objective {
    public string targetObject;
    bool objectHasBeenUsed;
    public override ObjectiveDelta ToDelta(LevelState state) {
        string targetIdn = SelectSpawnPointIdn(state.template, state.plan);

        ObjectiveLootSpawnpoint spawnpoint = state.spawnPoints[targetIdn];

        ObjectiveDelta delta = new ObjectiveDelta(this, () => spawnpoint.transform.position);
        if (state.plan.objectiveLocations.ContainsKey(name)) {
            delta.visibility = Visibility.known;
        }

        Interactive interactive = spawnpoint.GetComponentInChildren<Interactive>();
        interactive.OnUsed += () => delta.status = ObjectiveStatus.complete;

        return delta;
    }
}