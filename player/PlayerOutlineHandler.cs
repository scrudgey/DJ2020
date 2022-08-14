using System.Collections;
using System.Collections.Generic;
using System.Linq;
using cakeslice;
using UnityEngine;
public class PlayerOutlineHandler : MonoBehaviour {
    public List<Outline> outlines;

    public float period = 0.1f;
    public int cycles = 3;
    public float timer;
    public float maxTime = 2;

    public void Bind() {
        GameManager.OnSuspicionChange += HandleSuspicionChange;
        HandleSuspicionChange();
    }
    public void UnBind() {
        GameManager.OnSuspicionChange -= HandleSuspicionChange;
    }
    void OnDestroy() {
        UnBind();
    }
    public void HandleSuspicionChange() {
        Suspiciousness sus = GameManager.I.GetTotalSuspicion();
        SetOutlineColor(sus);
        if (sus == Suspiciousness.normal) {
            // DisableOutlines();
            // timer = -1f;
        } else if (sus == Suspiciousness.suspicious) {
            if (timer < 0) {
                EnableOutlines();
                timer = 0f;
            }
        } else if (sus == Suspiciousness.aggressive) {
            if (timer < 0) {
                EnableOutlines();
                timer = 0f;
            }
        }
    }
    void Update() {
        if (timer >= 0) {
            timer += Time.deltaTime;
            if (timer > maxTime) {
                DisableOutlines();
                timer = -1f;
            } else if (timer > period * cycles) {
                // do nothing
            } else {
                if (Mathf.Cos(timer * 6.28f / period) > 0) {
                    EnableOutlines();
                } else DisableOutlines();
            }
        }
    }
    void EnableOutlines() {
        outlines.ForEach(outline => outline.enabled = true);
    }
    void DisableOutlines() {
        outlines.ForEach(outline => outline.enabled = false);
    }
    void SetOutlineColor(Suspiciousness sus) {
        foreach (Outline outline in outlines) {
            outline.color = sus switch {
                Suspiciousness.normal => 1,
                Suspiciousness.suspicious => 2,
                Suspiciousness.aggressive => 0,
                _ => 0
            };
        }
    }
}
