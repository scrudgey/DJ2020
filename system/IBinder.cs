using UnityEngine;

public interface IBinder<T>
where T : Component, IBindable<T> {
    public T target { get; set; }
    public void Bind(GameObject newTargetObject) {
        if (target != null) {
            target.OnValueChanged -= HandleValueChanged;
        }
        target = newTargetObject.GetComponentInChildren<T>();
        if (target != null) {
            target.OnValueChanged += HandleValueChanged;
            HandleValueChanged(target);
        }
    }

    public abstract void HandleValueChanged(T t);
}