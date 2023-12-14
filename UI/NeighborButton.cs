using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class NeighborButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    public NodeInfoPaneDisplay display;
    public TextMeshProUGUI text;
    string idn;
    public void Configure<U>(NodeInfoPaneDisplay display, Node<U> neighbor) where U : Node<U> {
        this.display = display;
        this.idn = neighbor.idn;
        text.text = neighbor.idn.Substring(0, 10);
    }
    public void Click() {
        display.NeighborButtonClick(idn);
    }
    public virtual void OnPointerEnter(PointerEventData eventData) {
        display.NeighborButtonMouseOver(idn);
    }
    public virtual void OnPointerExit(PointerEventData eventData) {
        display.NeighborButtonMouseExit(idn);
    }
}
