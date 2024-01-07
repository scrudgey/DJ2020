using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class ManualHackInput {
    public PlayerInput playerInput;
    public Items.ItemInstance activeItem;
}
public class ManualHackTargetData {
    public CyberComponent target;
    public ManualHackTargetData(CyberComponent target) {
        this.target = target;
    }
    public HackInput ToManualHackInput() => new HackInput {
        targetNode = GameManager.I.GetCyberNode(target.idn),
        type = HackType.manual
    };
}
public class ManualHacker : MonoBehaviour {
    public bool deployed;
    Transform myTransform;
    public CyberNode targetNode;
    public GameObject visualEffect;
    public GameObject discoveryParticleEffect;
    PrefabPool discoveryParticlePool;
    [Header("configure")]
    public LineRenderer lineRenderer;
    public Material wireUnfurlMaterial;
    public Material wireAttachedMaterial;
    private readonly AnimationCurve fatWidth = AnimationCurve.Constant(0f, 1f, 1f);
    private readonly AnimationCurve thinWidth = AnimationCurve.Constant(0f, 1f, 0.02f);
    float timer;
    public Color wireColor;
    public AudioSource audioSource;
    public AudioClip wireDeploy;
    public AudioClip wireAttach;
    public AudioClip discoverySound;
    bool deploySoundPlayed;
    bool attachSoundPlayed;
    void Start() {
        myTransform = transform;
        audioSource = Toolbox.SetUpAudioSource(gameObject);
        targetNode = null;
        discoveryParticlePool = PoolManager.I.GetPool(discoveryParticleEffect);
    }
    void Update() {
        if (targetNode != null) {
            timer += Time.deltaTime;
            UpdateWire(targetNode);
            if (Vector3.Distance(myTransform.position, targetNode.position) > 3f) {
                Disconnect();
            }
        } else {
            timer = 0f;
            lineRenderer.enabled = false;
        }
        visualEffect.SetActive(deployed);
        if (deployed) {
            Vector3 playerPosition = GameManager.I.playerPosition;
            foreach (CyberNode node in GameManager.I.gameData.levelState.delta.cyberGraph.nodes.Values) {
                float distance = Vector3.Distance(node.position, playerPosition);
                if (distance < 3) {
                    bool wasDiscovered = node.BeDiscovered();
                    if (wasDiscovered) {
                        Toolbox.RandomizeOneShot(audioSource, discoverySound);
                        discoveryParticlePool.GetObject(node.position);
                    }
                }
            }
        }
    }


    public void Connect(CyberNode target) {
        if (!deployed) return;
        if (Vector3.Distance(target.position, transform.position) > 3f) return;
        Disconnect(dontRefreshCyberGraph: true);
        targetNode = target;
        target.isManualHackerTarget = true;
        GameManager.I.RefreshCyberGraph();
    }
    public void Disconnect(bool dontRefreshCyberGraph = false) {
        if (targetNode != null) {
            targetNode.isManualHackerTarget = false;
        }
        targetNode = null;
        if (!dontRefreshCyberGraph) {
            GameManager.I.RefreshCyberGraph();
        }
        deploySoundPlayed = false;
        attachSoundPlayed = false;
    }

    void UpdateWire(CyberNode node) {
        Vector3 playerPos = myTransform.position;
        lineRenderer.enabled = true;
        Vector3[] points = new Vector3[2];
        if (timer < 0.25) {
            Vector3 direction = node.position - playerPos;
            float length = 0.15f + 0.65f * (timer / 0.25f);
            points = new Vector3[]{
                    playerPos,
                    playerPos + length * direction.normalized
                };
            lineRenderer.material = wireUnfurlMaterial;
            lineRenderer.widthCurve = fatWidth;
            lineRenderer.startColor = Color.white;
            lineRenderer.endColor = Color.white;

            if (!deploySoundPlayed) {
                deploySoundPlayed = true;
                Toolbox.RandomizeOneShot(audioSource, wireDeploy);
            }
        } else {
            points = new Vector3[]{
                    node.position,
                    playerPos
                };
            lineRenderer.material = wireAttachedMaterial;
            lineRenderer.widthCurve = thinWidth;
            lineRenderer.startColor = wireColor;
            lineRenderer.endColor = wireColor;

            if (!attachSoundPlayed) {
                attachSoundPlayed = true;
                Toolbox.RandomizeOneShot(audioSource, wireAttach);
            }
        }
        lineRenderer.SetPositions(points);
        lineRenderer.positionCount = 2;
    }
}
