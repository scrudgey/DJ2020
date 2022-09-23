using UnityEngine;
namespace AI {

    public interface IState {
        public void Enter();
        public PlayerInput Update(ref PlayerInput input);
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
            PlayerInput input = PlayerInput.none;
            return currentState?.Update(ref input) ?? input;
        }
    }
}
