using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class NeighborButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    public Button button;
    Action<string> neighborClick;
    Action<string> neighborMouseover;
    Action neighborMouseExit;
    public TextMeshProUGUI text;
    string idn;
    EdgeVisibility edgeVisibility;
    public void Configure<T, U, V>(NodeInfoPaneDisplay<T, U, V> display, Node<U> neighbor, EdgeVisibility visibility, NodeVisibility nodeVisibility) where T : Graph<U, T> where U : Node<U> where V : NodeIndicator<U, T> {
        // this.display = display;
        neighborClick = display.NeighborButtonClick;
        neighborMouseover = display.NeighborButtonMouseOver;
        neighborMouseExit = display.NeighborButtonMouseExit;
        this.idn = neighbor.idn;
        this.edgeVisibility = visibility;
        // text.text = neighbor.idn.Substring(0, Math.Min(10, neighbor.idn.Length));
        text.text = neighbor.nodeTitle;
        if (visibility == EdgeVisibility.unknown) {
            MakeInactive();
        }
    }
    public void MakeInactive() {
        button.interactable = false;
        text.text = "???";
    }
    public void Click() {
        neighborClick.Invoke(idn);
    }
    public virtual void OnPointerEnter(PointerEventData eventData) {
        if (edgeVisibility == EdgeVisibility.unknown) return;
        neighborMouseover.Invoke(idn);
    }
    public virtual void OnPointerExit(PointerEventData eventData) {
        if (edgeVisibility == EdgeVisibility.unknown) return;
        neighborMouseExit.Invoke();
    }
}
