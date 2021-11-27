using System;
using UnityEngine;
public interface IBindable<T> where T : Component {
    public Action<T> OnValueChanged { get; set; }
}