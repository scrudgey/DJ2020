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
    private void OnTriggerEnter(Collider other) {
        newestAddition = other;
        fieldOfView.Add(other);
        OnValueChanged?.Invoke(this);
    }
    private void OnTriggerExit(Collider other) {
        newestRemoval = other;
        fieldOfView.Remove(other);
        OnValueChanged?.Invoke(this);
    }

}
