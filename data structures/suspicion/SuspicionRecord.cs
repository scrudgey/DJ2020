public class SuspicionRecord {
    public string content;
    public float lifetime;
    public float maxLifetime;
    public Suspiciousness suspiciousness;
    public void Update(float deltaTime) {
        lifetime -= deltaTime;
    }

    public bool IsTimed() {
        return maxLifetime > 0;
    }
}