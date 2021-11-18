using UnityEngine;

[ExecuteInEditMode]
public class TriggerSubEmitter : MonoBehaviour {
    private ParticleSystem ps;
    private float m_Timer = 0.0f;
    public float m_Interval = 0.1f;

    void Awake() {
        ps = GetComponent<ParticleSystem>();
    }

    private void Update() {
        m_Timer += Time.deltaTime;
        while (m_Timer >= m_Interval) {
            ps.TriggerSubEmitter(0);
            m_Timer -= m_Interval;
        }
    }
}