using System;
using UnityEngine;
public interface IBindable<T> where T : MonoBehaviour {
    public Action<T> OnValueChanged { get; set; }
}