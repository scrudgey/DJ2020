using UnityEngine;

public interface IState {
    public void Enter();
    public PlayerInput Update();
    public void Exit();
}

public class StateMachine<T> where T : IState {
    public T currentState;
    public float timeInCurrentState;
    public void ChangeState(T newState) {
        if (currentState != null)
            currentState.Exit();
        timeInCurrentState = 0f;
        currentState = newState;
        currentState.Enter();
    }

    public PlayerInput Update() {
        timeInCurrentState += Time.deltaTime;
        if (currentState != null) return currentState.Update();
        return new PlayerInput();
    }
}
