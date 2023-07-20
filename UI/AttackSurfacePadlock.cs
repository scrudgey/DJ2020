using System;
using System.Collections;
using System.Collections.Generic;
using Easings;
using UnityEngine;
public class AttackSurfacePadlock : AttackSurfaceLock {
    enum State { normal, disclose, open }
    State state;
    [Header("padlock")]
    public AudioClip[] openSounds;
    public AudioClip[] removeSounds;
    public AudioClip[] discloseSounds;
    public AudioClip[] clackSounds;

    public GameObject basePadlock;
    public GameObject disclosePadlock;
    Coroutine transitionRoutine;
    public Transform basePadlockRect;
    public Transform disclosePadlockRect;
    Vector3 initialPosition;
    bool isOpen;
    public void Start() {
        // base.Start();
        initialPosition = basePadlockRect.localPosition;
    }
    override public void OnMouseOver() {
        if (isOpen) return;
        ChangeState(State.disclose);
    }
    override public void OnMouseExit() {
        if (isOpen) return;
        ChangeState(State.normal);
    }
    void ChangeState(State newState) {
        if (newState == state) return;
        State tempInitialState = state;
        OnStateExit(tempInitialState, newState);
        state = newState;
        OnStateEnter(tempInitialState, newState);
    }
    void OnStateEnter(State fromState, State toState) {
        switch (toState) {
            case State.disclose:
                Transition(DiscloseRoutine);
                break;
            case State.normal:
                Transition(ReturnToBase);
                break;
            default:
                break;
        }
    }
    void OnStateExit(State fromState, State toState) {
        switch (fromState) {
            default:
                break;
        }
    }

    void Transition(Func<IEnumerator> coroutine) {
        if (transitionRoutine != null) {
            StopCoroutine(transitionRoutine);
        }
        transitionRoutine = StartCoroutine(Toolbox.ChainCoroutines(coroutine(), ClearTransitionRoutine()));
    }

    IEnumerator ClearTransitionRoutine() {
        transitionRoutine = null;
        yield return null;
    }

    IEnumerator DiscloseRoutine() {
        basePadlock.SetActive(true);
        disclosePadlock.SetActive(false);
        basePadlockRect.localPosition = initialPosition;
        disclosePadlockRect.localPosition = initialPosition;
        Toolbox.RandomizeOneShot(audioSource, discloseSounds);
        yield return new WaitForSecondsRealtime(0.05f);
        basePadlockRect.localPosition = initialPosition + new Vector3(0f, 0.02f, 0f);
        yield return new WaitForSecondsRealtime(0.05f);
        basePadlock.SetActive(false);
        disclosePadlock.SetActive(true);
        yield return new WaitForSecondsRealtime(0.05f);
        disclosePadlockRect.localPosition = initialPosition + new Vector3(0f, 0.03f, 0f);
    }
    IEnumerator ReturnToBase() {
        basePadlock.SetActive(false);
        disclosePadlock.SetActive(true);
        basePadlockRect.localPosition = initialPosition + new Vector3(0f, 0.02f, 0f);
        disclosePadlockRect.localPosition = initialPosition + new Vector3(0f, 0.03f, 0f);
        Toolbox.RandomizeOneShot(audioSource, discloseSounds);

        yield return new WaitForSecondsRealtime(0.05f);
        disclosePadlockRect.localPosition = initialPosition;
        yield return new WaitForSecondsRealtime(0.05f);
        basePadlock.SetActive(true);
        disclosePadlock.SetActive(false);
        yield return new WaitForSecondsRealtime(0.05f);
        basePadlockRect.localPosition = initialPosition;
    }

    IEnumerator OpenLock() {
        basePadlock.SetActive(true);
        disclosePadlock.SetActive(false);
        basePadlockRect.localPosition = initialPosition;
        disclosePadlockRect.localPosition = initialPosition;
        Toolbox.RandomizeOneShot(audioSource, openSounds);
        yield return new WaitForSecondsRealtime(0.1f);
        basePadlockRect.localPosition = initialPosition + new Vector3(0f, 0.02f, 0f);
        yield return new WaitForSecondsRealtime(0.1f);
        basePadlockRect.localPosition = initialPosition + new Vector3(0f, -0.04f, 0f);
        Toolbox.RandomizeOneShot(audioSource, removeSounds);
        yield return new WaitForSecondsRealtime(0.15f);
        float timer = 0f;
        float duration = 0.75f;
        while (timer < duration) {
            timer += Time.unscaledDeltaTime;
            float angle = (float)PennerDoubleAnimation.Linear(timer, 0f, 180f, duration);
            float x = (float)PennerDoubleAnimation.Linear(timer, 0f, -0.1f, duration);
            Quaternion rotation = Quaternion.Euler(0f, angle, 0f);
            basePadlockRect.rotation = rotation;

            basePadlockRect.localPosition = initialPosition + new Vector3(x, -0.04f, 0f);
            yield return null;
        }
        yield return new WaitForSecondsRealtime(0.15f);
        Destroy(gameObject);
        OnValueChanged?.Invoke(this);
    }

    IEnumerator TestHasp() {
        basePadlock.SetActive(false);
        disclosePadlock.SetActive(true);
        basePadlockRect.localPosition = initialPosition;
        disclosePadlockRect.localPosition = initialPosition;
        // Toolbox.RandomizeOneShot(audioSource, openSounds);
        yield return new WaitForSecondsRealtime(0.05f);
        basePadlock.SetActive(true);
        disclosePadlock.SetActive(false);
        yield return new WaitForSecondsRealtime(0.15f);
        basePadlockRect.localPosition = initialPosition + new Vector3(0f, 0.01f, 0f);
        Toolbox.RandomizeOneShot(audioSource, clackSounds);
        yield return new WaitForSecondsRealtime(0.1f);
        basePadlockRect.localPosition = initialPosition + new Vector3(0f, -0.01f, 0f);
        yield return new WaitForSecondsRealtime(0.1f);
        basePadlockRect.localPosition = initialPosition + new Vector3(0f, 0.01f, 0f);
        Toolbox.RandomizeOneShot(audioSource, clackSounds);
        yield return new WaitForSecondsRealtime(0.1f);
        basePadlockRect.localPosition = initialPosition + new Vector3(0f, -0.01f, 0f);
        yield return new WaitForSecondsRealtime(0.15f);
        basePadlockRect.localPosition = initialPosition + new Vector3(0f, 0.01f, 0f);
    }

    override protected BurglarAttackResult DoPick() {
        isOpen = true;
        Transition(OpenLock);
        return base.DoPick();
    }

    public override BurglarAttackResult HandleSingleClick(BurglarToolType activeTool, BurgleTargetData data) {
        if (activeTool == BurglarToolType.none) {
            Transition(TestHasp);
            return BurglarAttackResult.None;
        }
        return base.HandleSingleClick(activeTool, data);
    }
}
