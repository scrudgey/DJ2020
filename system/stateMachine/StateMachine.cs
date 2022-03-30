using UnityEngine;

public interface IState {
    public void Enter();
    public PlayerInput Update();
    public void Exit();
}

public class StateMachine<T> where T : IState {
    public T currentState;

    public void ChangeState(T newState) {
        if (currentState != null)
            currentState.Exit();

        currentState = newState;
        currentState.Enter();
    }

    public PlayerInput Update() {
        if (currentState != null) return currentState.Update();
        return new PlayerInput();
    }
}
