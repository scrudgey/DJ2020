using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHitstateSubscriber {
    public HitState hitState { get; set; }
    public void OnHitStateExit(HitState state, HitState toState) { }
    public void OnHitStateEnter(HitState state, HitState fromState) { }
    public void HandleHurtableChanged(Destructible hurtable) {
        TransitionToHitState(hurtable.hitState);
    }
    public void TransitionToHitState(HitState newState) {
        if (newState == hitState) {
            return;
        }
        HitState tmpInitialState = hitState;
        OnHitStateExit(tmpInitialState, newState);
        hitState = newState;
        OnHitStateEnter(newState, tmpInitialState);
    }
}
