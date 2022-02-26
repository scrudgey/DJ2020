using UnityEngine;

public interface IBinder<T> where T : MonoBehaviour, IBindable<T> {
    T target { get; set; }
    void Bind(GameObject newTargetObject) {
        if (newTargetObject == null)
            return;
        // Debug.Log($"{this} binding to target {newTargetObject}");
        if (target != null && target.OnValueChanged != null)
            target.OnValueChanged -= HandleValueChanged;

        target = newTargetObject.GetComponentInChildren<T>();
        if (target != null) {
            target.OnValueChanged += HandleValueChanged;
            // HandleValueChanged(target);
        }
    }

    void HandleValueChanged(T t);
}