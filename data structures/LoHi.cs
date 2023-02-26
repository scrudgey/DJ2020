using UnityEngine;

[System.Serializable]
public class LoHi {
    public float low;
    public float high;
    public LoHi() : this(0.025f, 0.075f) { }
    public LoHi(float low, float high) {
        this.low = low;
        this.high = high;
    }
    public float GetRandomInsideBound() => UnityEngine.Random.Range(low, high);

    public float Average() => (low + high) / 2f;
}
