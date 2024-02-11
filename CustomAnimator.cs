using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    Stack<AnimationEvent> nextEvents = new Stack<AnimationEvent>();
    float referenceTime;
    public void Stop() {
        isPlaying = false;
    }
    public void Play() {
        if (!isPlaying) {
            referenceTime = Time.time;
        }
        isPlaying = true;
    }
    void Start() {
    }
    void OnClipChange() {
        timer = 0f;
        nextEvents = new Stack<AnimationEvent>();
        ResetStack();
    }

    void Update() {
        if (isPlaying && _clip != null) {
            timer = (Time.time - referenceTime) * playbackSpeed;
            while (nextEvents.Count > 0 && timer > nextEvents.Peek().time && isPlaying) {
                FireEvent(nextEvents.Pop());
            }
            if (timer > _clip.length && _clip.length > 0) {
                if (_clip.wrapMode == WrapMode.Default || _clip.wrapMode == WrapMode.Once) {
                    Stop();
                } else {
                    // Debug.Log($"reset:\t{playbackSpeed}\t{timer} ");
                    ResetStack();
                }
            }
        }
    }
    void ResetStack() {
        foreach (AnimationEvent e in _clip.events.Reverse()) {
            if (e.time == 0) {
                FireEvent(e);
            } else {
                nextEvents.Push(e);
            }
        }
        // TODO: handle overflow time on looping clips
        referenceTime = Time.time;
    }
    void FireEvent(AnimationEvent animationEvent) {
        BroadcastMessage(animationEvent.functionName, animationEvent.intParameter, SendMessageOptions.DontRequireReceiver);
    }
}
