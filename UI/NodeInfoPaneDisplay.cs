using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class NodeInfoPaneDisplay<T, U, V> : MonoBehaviour where T : Graph<U, T> where U : Node<U> where V : NodeIndicator<U, T> {
    public TextMeshProUGUI title;
    public TextMeshProUGUI lockStatus;
    public Image icon;
    public GameObject neighborContainer;
    public Transform neighborbuttonContainer;
    public GameObject neighborButtonPrefab;
    [HideInInspector]
    public U node;
    [HideInInspector]
    public GraphOverlay<T, U, V> handler;
    [HideInInspector]
    public V indicator;
    public Sprite mysteryIcon;
    public Color mysteryColor;


    // TODO: support visibility
    public void Configure(V indicator, T graph, GraphOverlay<T, U, V> handler) {
        this.node = indicator.node;
        this.handler = handler;
        this.indicator = indicator;
        title.text = node.nodeTitle;
        icon.sprite = indicator.iconImage.sprite;
        if (node.visibility == NodeVisibility.mystery) {
            ConfigureMysteryNode();
            foreach (Transform child in neighborbuttonContainer) {
                Destroy(child.gameObject);
            }
        } else {
            ConfigureNode();
            ConfigureNeighbors(graph);
        }
    }
    public abstract void ConfigureNode();
    public abstract void ConfigureMysteryNode();
    protected void ConfigureNeighbors(T graph) {
        foreach (Transform child in neighborbuttonContainer) {
            Destroy(child.gameObject);
        }
        if (graph.edges.ContainsKey(node.idn) && graph.edges[node.idn].Count > 0) {
            foreach (string neighborId in graph.edges[node.idn]) {
                U node = graph.nodes[neighborId];
                if (node.visibility == NodeVisibility.unknown) continue; // TODO: populate ? placeholder
                GameObject obj = GameObject.Instantiate(neighborButtonPrefab) as GameObject;
                obj.transform.SetParent(neighborbuttonContainer, false);
                NeighborButton button = obj.GetComponent<NeighborButton>();
                button.Configure(this, node);
            }
            neighborContainer.SetActive(true);
        } else {
            neighborContainer.SetActive(false);
        }
    }

    public void NeighborButtonClick(string idn) {
        handler.NeighborButtonClick(idn);
    }
    public void NeighborButtonMouseOver(string idn) {
        handler.NeighborButtonMouseOver(node.idn, idn);
    }
    public void NeighborButtonMouseExit() {
        handler.NeighborButtonMouseExit();
    }
    public void MouseOverScrollBox() {
        handler.overlayHandler.uIController.mouseOverScrollBox = true;
    }
    public void MouseExitScrollBox() {
        handler.overlayHandler.uIController.mouseOverScrollBox = false;
    }
}