using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// TODO: rename to HackPanelHandler
public class HackHandler : MonoBehaviour, IBinder<HackController> {
    public HackController target { get; set; }
    public Transform entriesHolder;
    HackController hackTarget;
    public GameObject hackPanelEntryPrefab;
    List<HackPanelEntry> entries = new List<HackPanelEntry>();

    void Start() {
        foreach (Transform child in entriesHolder) {
            Destroy(child.gameObject);
        }
        entries = new List<HackPanelEntry>();
        for (int i = 0; i < 5; i++) {
            GameObject newEntry = GameObject.Instantiate(hackPanelEntryPrefab);
            newEntry.transform.SetParent(entriesHolder, false);
            HackPanelEntry hackPanelEntry = newEntry.GetComponent<HackPanelEntry>();
            hackPanelEntry.Clear();
            newEntry.SetActive(false);
            entries.Add(hackPanelEntry);
        }
        ((IBinder<HackController>)this).Bind(HackController.I.gameObject);
    }
    public void HandleValueChanged(HackController hackController) {
        if (hackController.targets.Count == 0) {
            gameObject.SetActive(false);
        } else {
            gameObject.SetActive(true);
        }
        for (int i = 0; i < hackController.targets.Count; i++) {
            HackPanelEntry entry = entries[i];
            entry.gameObject.SetActive(true);
            entry.Configure(hackController.targets[i]);
        }
        for (int i = hackController.targets.Count; i < 5; i++) {
            HackPanelEntry entry = entries[i];
            entry.Clear();
            entry.gameObject.SetActive(false);
        }
    }
}
