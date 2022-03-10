public class SphereRobotBrain : StateMachine<SphereControlState> {
    public PlayerInput getInput() {
        if (currentState != null) {
            return currentState.getInput();
        } else return new PlayerInput();
    }
}