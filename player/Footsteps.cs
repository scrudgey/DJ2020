using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Footsteps : MonoBehaviour {
    public AudioSource audioSource;
    public AudioClip[] grassSounds;
    public AudioClip[] defaultSounds;
    public AudioClip[] metalSounds;
    public AudioClip[] bushSounds;
    public float timer;
    private AudioClip leftFoot;
    private AudioClip rightFoot;
    private bool onRightFoot;
    private SurfaceType lastSurfaceType;
    void Awake() {
        audioSource = Toolbox.SetUpAudioSource(gameObject);
    }
    public void UpdateWithVelocity(Vector3 velocity) {
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
                if (onRightFoot) {
                    Toolbox.RandomizeOneShot(audioSource, rightFoot);
                } else {
                    Toolbox.RandomizeOneShot(audioSource, leftFoot);
                }
                NoiseData noise = new NoiseData {
                    player = true,
                    suspiciousness = Suspiciousness.normal,
                    volume = 1
                };
                Toolbox.Noise(transform.position, noise, player: true);
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
        RaycastHit[] hits = Physics.RaycastAll(transform.position, -1f * transform.up, 0.5f); // get all hits
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
        AudioClip[] soundSet = defaultSounds;
        switch (surfaceType) {
            case SurfaceType.grass:
                soundSet = grassSounds;
                break;
            case SurfaceType.metal:
                soundSet = metalSounds;
                break;
            case SurfaceType.tree:
                soundSet = bushSounds;
                break;
        }
        leftFoot = Toolbox.RandomFromList(soundSet);
        rightFoot = Toolbox.RandomFromList(soundSet);
    }
}
