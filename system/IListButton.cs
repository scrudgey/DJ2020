// using UnityEngine;
// public abstract class ButtonListController<T, U> : MonoBehaviour where U : ListButton<U, T> where T : ButtonListController<T, U> {
//     public Transform buttonContainer;
//     public GameObject buttonPrefab;
//     public void InitializeButtons() {
//         foreach (Transform child in buttonContainer) {
//             GameObject.Destroy(child.gameObject);
//         }
//         CreateAllButtons();
//     }
//     protected U CreateButton() {
//         GameObject obj = GameObject.Instantiate(buttonPrefab);
//         obj.transform.SetParent(buttonContainer);
//         U button = obj.GetComponent<U>();
//         return button;
//     }
//     public abstract void CreateAllButtons();
//     public virtual void ButtonCallback(ListButton<U, T> button) { }
// }

// public class ListButton<T, U> : MonoBehaviour where U : ButtonListController<U, T> where T : ListButton<T, U> {
//     U controller;
//     public void Initialize(U controller) {
//         this.controller = controller;
//     }
//     public void OnClick() {
//         controller.ButtonCallback(this);
//     }
// }