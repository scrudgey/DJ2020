using System.Collections.Generic;
using UnityEngine;

public class AnimateFrames : MonoBehaviour {
    public List<Sprite> frames;
    private SpriteRenderer spriteRenderer;
    private float animationTimer;
    public float frameTime = 1f;
    private int frameIndex = 0;
    AudioSource audioSource;
    public AudioClip flipSound;
    public bool destroyOnFinished;
    void Start() {
        audioSource = Toolbox.SetUpAudioSource(gameObject);
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) {
            Destroy(this);
        }
        spriteRenderer.sprite = frames[0];
        audioSource = GetComponent<AudioSource>();
    }
    void Update() {
        animationTimer += Time.deltaTime;
        if (animationTimer > frameTime) {
            frameIndex += 1;
            if (frameIndex == frames.Count) {
                frameIndex = 0;
                if (destroyOnFinished) {
                    // Destroy(gameObject);
                    PoolManager.I.RecallObject(gameObject);
                } else {
                    if (audioSource != null && flipSound != null)
                        audioSource.PlayOneShot(flipSound);
                }
            }
            animationTimer = 0f;
            spriteRenderer.sprite = frames[frameIndex];
        }
    }
}