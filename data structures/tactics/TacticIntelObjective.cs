using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]

[CreateAssetMenu(menuName = "ScriptableObjects/Tactics/TacticIntelObjective")]
public class TacticIntelObjective : Tactic {
    public override void ApplyPurchaseState(LevelTemplate template, LevelPlan plan) {
        base.ApplyPurchaseState(template, plan);
        foreach (Objective objective in template.AllObjectives()) {
            plan.SetObjectiveLocation(objective);
        }
    }
}