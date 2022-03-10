using UnityEngine;

public interface IState {
    public void Enter();
    public void Update();
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

    public void Update() {
        if (currentState != null) currentState.Update();
    }
}
