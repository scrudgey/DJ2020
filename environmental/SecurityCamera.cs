using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Easings;
using UnityEngine;
public class SecurityCamera : IBinder<SightCone>, INodeBinder<AlarmNode> {
    enum State { rotateLeft, lookLeft, rotateRight, lookRight }
    State state;
    public AlarmNode node { get; set; }
    public Transform cameraTransform;
    public SightCone sightCone;
    public Transform sightOrigin;
    public AlertHandler alertHandler;
    public SpriteRenderer spriteLight;
    public MeshRenderer IRCone;
    [Header("Sounds")]
    public AudioSource audioSource;
    public AudioClip[] rotateSound;
    public AudioClip[] spottedSound;
    float cooldown;
    [Header("Rotation")]
    float timer;
    public LoHi angleBounds;
    public bool doRotate = true;
    float angle;
    Quaternion initialRotation;
    Quaternion rotation;
    public float turnDuration;
    public float lookDuration;
    readonly float MAXIMUM_SIGHT_RANGE = 50f;
    public AttackSurfaceElement[] attachedElements;

    void Awake() {
        audioSource = Toolbox.SetUpAudioSource(gameObject);
        audioSource.volume = 0.5f;
    }
    void Start() {
        UpdateEnabled();
        Bind(sightCone.gameObject);
        initialRotation = cameraTransform.rotation;
    }
    public void HandleNodeChange() {
        UpdateEnabled();
    }

    void UpdateEnabled() {
        if (node == null) return;
        if (spriteLight != null)
            spriteLight.enabled = node.overallEnabled;
        if (IRCone != null)
            IRCone.enabled = node.overallEnabled;
        if (!node.overallEnabled) {
            if (audioSource != null)
                audioSource.Stop();
        }
    }
    override public void OnDestroy() {
        base.OnDestroy();
    }
    public override void HandleValueChanged(SightCone t) {
        if (node == null || !node.overallEnabled)
            return;
        if (t.newestAddition != null) {
            if (TargetVisible(t.newestAddition))
                Perceive(t.newestAddition);
        }
    }
    bool TargetVisible(Collider other) {
        Vector3 position = sightOrigin.position; // TODO: configurable
        Vector3[] directions = new Vector3[]{
            other.bounds.center - position,
            (other.bounds.center + other.bounds.extents) - position,
            (other.bounds.center - other.bounds.extents) - position,
        };
        foreach (Vector3 direction in directions) {
            Ray ray = new Ray(position, direction);
            RaycastHit[] hits = Physics.RaycastAll(ray, MAXIMUM_SIGHT_RANGE, LayerUtil.GetLayerMask(Layer.def, Layer.obj, Layer.interactive));
            foreach (RaycastHit hit in hits.OrderBy(h => h.distance)) {
                if (hit.transform.IsChildOf(transform.root))
                    continue;
                TagSystemData tagData = Toolbox.GetTagData(hit.collider.gameObject);
                if (tagData.bulletPassthrough) continue;

                Color color = other == hit.collider ? Color.yellow : Color.red;
                Debug.DrawLine(position, hit.collider.bounds.center, color, 0.5f);
                if (other == hit.collider || hit.transform.IsChildOf(other.transform.root)) {
                    return true;
                } else break;
            }
        }
        return false;
    }

    void Perceive(Collider other) {
        if (other.transform.IsChildOf(GameManager.I.playerObject.transform)) {
            PerceivePlayerObject(other);
        }
    }

    void PerceivePlayerObject(Collider other) {
        if (cooldown > 0f) {
            return;
        }
        float distance = Vector3.Distance(transform.position, other.bounds.center);
        if (GameManager.I.IsPlayerVisible(distance)) {
            GameManager.I.SetAlarmNodeTriggered(((INodeBinder<AlarmNode>)this).node, true);
            alertHandler.ShowAlert();
            cooldown = 5f;
            Toolbox.RandomizeOneShot(audioSource, spottedSound);
            GameManager.I.SetLocationOfDisturbance(other.bounds.center);
            GameManager.I.DispatchGuard(other.bounds.center);
            GameManager.I.AddSuspicionRecord(SuspicionRecord.trippedSensor("security camera"));
            GameManager.I.StickifySuspicion();
        }
    }

    void Update() {
        if (cooldown > 0f) {
            cooldown -= Time.deltaTime;
        }
        if (node == null || !node.overallEnabled)
            return;
        if (doRotate) {
            timer += Time.deltaTime;
            switch (state) {
                case State.rotateLeft:
                    angle = (float)PennerDoubleAnimation.Linear(timer, angleBounds.low, angleBounds.high - angleBounds.low, turnDuration);
                    rotation = Quaternion.AngleAxis(angle, Vector3.up);
                    cameraTransform.rotation = initialRotation * rotation;
                    if (timer > turnDuration) {
                        timer = 0f;
                        state = State.lookLeft;
                    }
                    foreach (AttackSurfaceElement element in attachedElements) {
                        if (element == null) continue;
                        element.ForceUpdate();
                    }
                    break;
                case State.lookLeft:
                    if (timer > lookDuration) {
                        timer = 0f;
                        state = State.rotateRight;
                        Toolbox.RandomizeOneShot(audioSource, rotateSound, randomPitchWidth: 0.05f);
                    }
                    break;
                case State.rotateRight:
                    angle = (float)PennerDoubleAnimation.Linear(timer, angleBounds.high, angleBounds.low - angleBounds.high, turnDuration);
                    rotation = Quaternion.AngleAxis(angle, Vector3.up);
                    cameraTransform.rotation = initialRotation * rotation;
                    if (timer > turnDuration) {
                        timer = 0f;
                        state = State.lookRight;
                    }
                    foreach (AttackSurfaceElement element in attachedElements) {
                        if (element == null) continue;
                        element.ForceUpdate();
                    }
                    break;
                case State.lookRight:
                    if (timer > lookDuration) {
                        timer = 0f;
                        state = State.rotateLeft;
                        Toolbox.RandomizeOneShot(audioSource, rotateSound, randomPitchWidth: 0.05f);
                    }
                    break;
            }
        }
    }

}
