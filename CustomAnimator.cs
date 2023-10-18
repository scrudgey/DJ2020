using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomAnimator : MonoBehaviour {
    private AnimationClip _legacyClip;
    public float playbackSpeed = 1f;
    public AnimationClip clip {
        set {
            _legacyClip = value;
            _clip = CustomAnimationClip.Load(value.name);
            OnClipChange();
        }
        get {
            return _legacyClip;
        }
    }
    private CustomAnimationClip _clip;
    bool isPlaying;
    float timer;
    AnimationEvent nextEvent;
    int eventIndex;
    public void Stop() {
        isPlaying = false;
    }
    public void Play() {
        isPlaying = true;
    }

    void Update() {
        if (isPlaying) {
            timer += Time.deltaTime * playbackSpeed;
            int j = 0;
            while (nextEvent != null && timer > nextEvent.time && j < 100) {
                FireEvent(nextEvent);
                eventIndex += 1;
                if (eventIndex > _clip.events.Length - 1) {
                    nextEvent = null;
                    eventIndex = 0;
                } else {
                    nextEvent = _clip.events[eventIndex];
                }
                j++;
            }
            // i think part of the problem is this being outside the other while loop
            while (timer > _clip.length) {
                timer -= _clip.length;
                eventIndex = 0;
                nextEvent = _clip.events[0];
                if (_clip.wrapMode == WrapMode.Default || _clip.wrapMode == WrapMode.Once) {
                    Stop();
                } else {

                }
                if (_clip.length <= 0) break;
            }
        }
    }
    void FireEvent(AnimationEvent animationEvent) {
        // Debug.Log($"Fire event: {animationEvent.functionName} {animationEvent.intParameter}");
        BroadcastMessage(animationEvent.functionName, animationEvent.intParameter, SendMessageOptions.DontRequireReceiver);
        // BroadcastMessage(animationEvent.functionName, animationEvent., SendMessageOptions.DontRequireReceiver);
    }
    void OnClipChange() {
        // AnimationUtility.
        timer = 0f;
        eventIndex = 0;
        nextEvent = _clip.events[0];
    }
}
