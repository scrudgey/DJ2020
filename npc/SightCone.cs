using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SightCone : MonoBehaviour, IBindable<SightCone> {
    public HashSet<Collider> fieldOfView = new HashSet<Collider>();
    public Collider newestAddition;
    public Collider newestRemoval;
    public Action<SightCone> OnValueChanged { get; set; }
    private Transform myRootTransform;
    void Start() {
        myRootTransform = transform.root;
    }
    private void OnTriggerEnter(Collider other) {
        // Debug.Log($"FOV enter: {other}");
        if (other.transform.root.IsChildOf(myRootTransform))
            return;
        newestAddition = other;
        fieldOfView.Add(other);
        PruneFieldOfView();
        OnValueChanged?.Invoke(this);
    }
    private void OnTriggerExit(Collider other) {
        // Debug.Log($"FOV exit: {other}");
        if (other.transform.root.IsChildOf(myRootTransform))
            return;
        newestRemoval = other;
        fieldOfView.Remove(other);
        PruneFieldOfView();
        OnValueChanged?.Invoke(this);
    }
    void PruneFieldOfView() => fieldOfView = fieldOfView.Where((collider) => collider != null).ToHashSet();

}
