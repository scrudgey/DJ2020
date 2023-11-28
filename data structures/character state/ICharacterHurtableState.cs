using UnityEngine;

public interface ICharacterHurtableState {
    public float health { get; set; }
    // public float fullHealthAmount { get; }
    public HitState hitState { get; set; }

    public void ApplyHurtableState(GameObject playerObject) {
        foreach (ICharacterHurtableStateLoader hurtableStateLoader in playerObject.GetComponentsInChildren<ICharacterHurtableStateLoader>()) {
            hurtableStateLoader.LoadCharacterState(this);
        }
    }
}