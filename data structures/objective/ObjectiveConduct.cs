using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]

[CreateAssetMenu(menuName = "ScriptableObjects/Objectives/ObjectiveConduct")]
public class ObjectiveConduct : Objective {
    public enum Type { dontKill, noalarm }
    public Type type;

    public override ObjectiveDelta ToDelta(LevelState state) {
        ObjectiveDelta delta = new ObjectiveDelta(this, () => Vector3.zero, "na") {
            hasLocation = false,
            status = ObjectiveStatus.complete
        };

        switch (type) {
            case Type.dontKill:
                GameManager.I.OnHumanDied += (CharacterController npc) => {
                    delta.status = ObjectiveStatus.failed;
                };
                break;
            case Type.noalarm:
                GameManager.I.OnAlarmActivate += () => {
                    delta.status = ObjectiveStatus.failed;
                };
                break;
        }

        return delta;
    }

}