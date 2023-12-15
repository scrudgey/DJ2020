using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
public class NeighborButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    // public CyberNodeInfoPaneDisplay display;
    Action<string> neighborClick;
    Action<string> neighborMouseover;
    Action neighborMouseExit;
    public TextMeshProUGUI text;
    string idn;
    public void Configure<T, U, V>(NodeInfoPaneDisplay<T, U, V> display, Node<U> neighbor) where T : Graph<U, T> where U : Node<U> where V : NodeIndicator<U, T> {
        // this.display = display;
        neighborClick = display.NeighborButtonClick;
        neighborMouseover = display.NeighborButtonMouseOver;
        neighborMouseExit = display.NeighborButtonMouseExit;
        this.idn = neighbor.idn;
        text.text = neighbor.idn.Substring(0, 10);
    }
    public void Click() {
        // display.NeighborButtonClick(idn);
        neighborClick.Invoke(idn);
    }
    public virtual void OnPointerEnter(PointerEventData eventData) {
        // display.NeighborButtonMouseOver(idn);
        neighborMouseover.Invoke(idn);
    }
    public virtual void OnPointerExit(PointerEventData eventData) {
        // display.NeighborButtonMouseExit(idn);
        neighborMouseExit.Invoke();
    }
}
