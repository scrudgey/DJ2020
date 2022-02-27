using UnityEngine;

public abstract class IBinder<T> : MonoBehaviour where T : MonoBehaviour, IBindable<T> {
    public T target;
    public virtual void Bind(GameObject newTargetObject) {
        if (newTargetObject == null)
            return;
        // Debug.Log($"{this} binding to target {newTargetObject}");
        if (target != null && target.OnValueChanged != null)
            target.OnValueChanged -= HandleValueChanged;

        target = newTargetObject.GetComponentInChildren<T>();
        if (target != null) {
            target.OnValueChanged += HandleValueChanged;
            HandleValueChanged(target);
        }
    }

    abstract public void HandleValueChanged(T t);
}