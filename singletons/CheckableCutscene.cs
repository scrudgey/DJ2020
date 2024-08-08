using System.Collections;

class CheckableCutscene {
    public bool isRunning;
    IEnumerator enumerator;
    public CheckableCutscene(IEnumerator enumerator) {
        this.enumerator = enumerator;
    }
    public IEnumerator Do() {
        isRunning = true;
        yield return enumerator;
        isRunning = false;
    }
}