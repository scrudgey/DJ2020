using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HackDisplay : IBinder<HackController> {
    // public HackController target { get; set; }
    public Camera cam;
    public GameObject hackIndicatorPrefab;
    List<HackIndicator> indicators = new List<HackIndicator>();

    void Start() {
        indicators = new List<HackIndicator>();
        for (int i = 0; i < 5; i++) {
            GameObject newEntry = GameObject.Instantiate(hackIndicatorPrefab);
            newEntry.transform.SetParent(transform, false);
            HackIndicator hackPanelEntry = newEntry.GetComponent<HackIndicator>();
            hackPanelEntry.cam = Camera.main; // bad
            hackPanelEntry.Clear();
            newEntry.SetActive(false);
            indicators.Add(hackPanelEntry);
        }
        Bind(HackController.I.gameObject);
        // HandleValueChanged(HackController.I);
    }
    override public void HandleValueChanged(HackController hackController) {
        int index = 0;
        if (hackController == null) {
            return;
        }

        if (hackController?.targets?.Count > 0) {
            // TODO: iterate
            foreach (HackController.HackData data in hackController.targets) {
                indicators[index].Configure(data.node, vulnerable: false, hacking: true);
                indicators[index].gameObject.SetActive(true);
                index++;
            }

        }

        if (hackController?.vulnerableManualNodes?.Count != 0) {
            // TODO: iterate
            foreach (CyberNode node in hackController.vulnerableManualNodes) {
                indicators[index].Configure(node, vulnerable: true, hacking: false);
                indicators[index].gameObject.SetActive(true);
                index++;

            }
        }

        if (hackController.vulnerableNetworkNode != null) {
            CyberNode nodeTarget = hackController.vulnerableNetworkNode;
            indicators[index].Configure(nodeTarget, vulnerable: true, hacking: false);
            indicators[index].gameObject.SetActive(true);
            index++;

        }
        for (int j = index; j < 5; j++) {
            indicators[j].Clear();
            indicators[j].gameObject.SetActive(true);
        }
    }
}
