using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayAnimation : MonoBehaviour {
    public Animation myAnimation;
    // public Animator myAnimator;
    public AnimationClip animationClip;
    public Animator animator;
    void Start() {
        // Time.timeScale = 1f;
        // // myAnimation.updateMode = AnimatorUpdateMode.UnscaledTime;
        // myAnimation.clip = animationClip;
        // myAnimation.Play();

        // Debug.Log(myAnimation.isPlaying);

        animator.Play("subway_jack", 0);
    }

}
