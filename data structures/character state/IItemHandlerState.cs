// using System.Collections.Generic;
// using UnityEngine;
// public interface IItemHandlerState {
//     public List<string> items { get; set; }

//     public void ApplyItemState(GameObject playerObject) {
//         foreach (IItemHandlerStateLoader itemLoader in playerObject.GetComponentsInChildren<IItemHandlerStateLoader>()) {
//             itemLoader.LoadItemState(this);
//         }
//     }
// }