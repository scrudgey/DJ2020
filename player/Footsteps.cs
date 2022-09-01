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
    void Awake() {
        audioSource = Toolbox.SetUpAudioSource(gameObject);
        audioSource.minDistance = 2f;
        audioSource.maxDistance = 10.42f;
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSource.spatialBlend = 1f;
        footstepData = Resources.Load("data/footstep/default") as FootstepData;
    }
    public void UpdateWithVelocity(Vector3 velocity, bool isRunning) {
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
                timer = 1f;

                float volume = lastSurfaceType switch {
                    SurfaceType.grass => 0.5f,
                    SurfaceType.tile => 2.8f,
                    SurfaceType.metal => 3f,
                    SurfaceType.normal => 1.5f,
                    SurfaceType.tree => 2f,
                    _ => 1.5f
                };
                if (isRunning) {
                    volume *= 3f;
                }

                if (onRightFoot) {
                    Toolbox.RandomizeOneShot(audioSource, rightFoot, volume: volume);
                } else {
                    Toolbox.RandomizeOneShot(audioSource, leftFoot, volume: volume);
                }

                NoiseData noise = new NoiseData {
                    player = gameObject == GameManager.I.playerObject,
                    suspiciousness = Suspiciousness.normal,
                    volume = volume
                };
                Toolbox.Noise(transform.position, noise);
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
        // TODO: use surface map.

        TagSystemData data = new TagSystemData();
        RaycastHit[] hits = Physics.RaycastAll(transform.position + 0.1f * Vector3.up, -1f * transform.up, 0.5f, LayerUtil.GetMask(Layer.def)); // get all hits
        foreach (RaycastHit hit in hits.OrderBy(h => h.distance)) { // check hits until a valid one is found
            if (hit.collider.isTrigger)
                continue;
            if (hit.collider.transform.IsChildOf(transform.root))
                continue;
            if (hit.collider.gameObject == null)
                continue;
            data = Toolbox.GetTagData(hit.collider.gameObject);
            break;
        }
        return data.surfaceSoundType;
    }
    public void SetFootstepSounds(SurfaceType surfaceType) {
        AudioClip[] soundSet = footstepData.GetSoundSet(surfaceType);
        leftFoot = Toolbox.RandomFromList(soundSet);
        rightFoot = Toolbox.RandomFromList(soundSet);
    }
}
