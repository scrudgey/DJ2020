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
    void Start() {
        audioSource = Toolbox.SetUpAudioSource(gameObject);
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
                timer = 1f + Random.Range(-0.1f, 0.1f);


                // TODO: adjust volume by velocity
                float volume = lastSurfaceType switch {
                    SurfaceType.grass => 1f,
                    SurfaceType.tile => 2.8f,
                    SurfaceType.metal => 4f,
                    SurfaceType.normal => 2f,
                    SurfaceType.tree => 2f,
                    _ => 1.5f
                };
                // if (isRunning) {
                //     volume *= 3f;
                // }
                volume *= velocity.magnitude / 2f;

                if (onRightFoot) {
                    Toolbox.RandomizeOneShot(audioSource, rightFoot, volume: volume);
                } else {
                    Toolbox.RandomizeOneShot(audioSource, leftFoot, volume: volume);
                }

                NoiseData noise = new NoiseData {
                    player = gameObject == GameManager.I.playerObject,
                    suspiciousness = Suspiciousness.normal,
                    volume = volume,
                    isFootsteps = true
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
