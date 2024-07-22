using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Footsteps : MonoBehaviour {
    public AudioSource audioSource;

    public float timer;
    private AudioClip leftFoot;
    private AudioClip rightFoot;
    private bool onRightFoot;
    private SurfaceType lastSurfaceType;
    private FootstepData footstepData;
    RaycastHit[] raycastHits;
    void Start() {
        raycastHits = new RaycastHit[1];
        audioSource = Toolbox.SetUpAudioSource(gameObject);
        audioSource.volume = 0.4f;
        // audioSource.maxDistance = 23f;
        footstepData = Resources.Load("data/footstep/default") as FootstepData;
    }
    public void UpdateWithVelocity(Vector3 velocity, bool isRunning) {
        if (footstepData == null) return;
        if (velocity.magnitude <= 0.01) {
            leftFoot = null;
            rightFoot = null;
        } else {
            timer -= velocity.magnitude * Time.deltaTime;
            if (leftFoot == null || rightFoot == null) {
                SetFootstepSounds(lastSurfaceType);
            }
            if (timer <= 0) {
                onRightFoot = !onRightFoot;
                timer = 1f + Random.Range(-0.1f, 0.1f);

                float volume = lastSurfaceType switch {
                    SurfaceType.grass => 1f,
                    SurfaceType.tile => 2.8f,
                    SurfaceType.metal => 4f,
                    SurfaceType.normal => 2f,
                    SurfaceType.tree => 2f,
                    SurfaceType.carpet => 0.75f,
                    _ => 1.5f
                };
                // if (isRunning) {
                //     volume *= 3f;
                // }
                volume *= velocity.magnitude / 1.25f;

                if (onRightFoot) {
                    Toolbox.RandomizeOneShot(audioSource, rightFoot, volume: volume / 3f);
                } else {
                    Toolbox.RandomizeOneShot(audioSource, leftFoot, volume: volume / 3f);
                }

                bool player = false;
                if (GameManager.I.gameData?.levelState?.delta != null) {
                    player = transform.IsChildOf(GameManager.I.playerObject.transform) &&
                                !(GameManager.I.gameData.levelState.delta.disguise && !GameManager.I.gameData.levelState.anyAlarmTerminalActivated());
                }


                NoiseData noise = new NoiseData {
                    player = player,
                    suspiciousness = Suspiciousness.normal,
                    volume = volume,
                    isFootsteps = true,
                    relevantParties = new HashSet<Transform>() { transform.root }
                };
                Toolbox.Noise(transform.position, noise, transform.root.gameObject, spherical: false);
                // if (player && GameManager.I.timePlayed > 3f) {
                //     Debug.Log(volume);
                //     Debug.Break();
                // }
            }
        }

    }
    public void FixedUpdate() {
        SurfaceType surfaceBelowMe = GetCurrentSurfaceType();
        if (surfaceBelowMe != lastSurfaceType) {
            SetFootstepSounds(surfaceBelowMe);
        }
        lastSurfaceType = surfaceBelowMe;
    }
    private SurfaceType GetCurrentSurfaceType() {
        // TODO: use surface map...?

        TagSystemData data = new TagSystemData();
        int numberHit = Physics.RaycastNonAlloc(transform.position + 0.1f * Vector3.up, -1f * transform.up, raycastHits, 0.5f, LayerUtil.GetLayerMask(Layer.def, Layer.obj), QueryTriggerInteraction.Ignore);
        if (numberHit > 0) {
            RaycastHit hit = raycastHits[0];
            data = Toolbox.GetTagData(hit.collider.gameObject);
        }
        return data.surfaceSoundType;
    }
    public void SetFootstepSounds(SurfaceType surfaceType) {
        if (footstepData == null) return;
        AudioClip[] soundSet = footstepData.GetSoundSet(surfaceType);
        leftFoot = Toolbox.RandomFromList(soundSet);
        rightFoot = Toolbox.RandomFromList(soundSet);
    }
}
