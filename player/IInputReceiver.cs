using UnityEngine;

public interface IInputReceiver {
    Transform transform { get; }
    public void SetInputs(PlayerInput input);
}