using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class HackHandler : MonoBehaviour {
    public Transform entriesHolder;
    HackController hackTarget;
    public GameObject hackPanelEntryPrefab;
    List<HackPanelEntry> entries = new List<HackPanelEntry>();

    public void Bind() {
        HackController.I.OnValueChanged += HandleValueChanged;
        HandleValueChanged();
    }
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
        Bind();
    }
    void HandleValueChanged() {
        for (int i = 0; i < HackController.I.targets.Count; i++) {
            HackPanelEntry entry = entries[i];
            entry.gameObject.SetActive(true);
            entry.Configure(HackController.I.targets[i]);
        }
        for (int i = HackController.I.targets.Count; i < 5; i++) {
            HackPanelEntry entry = entries[i];
            entry.Clear();
            entry.gameObject.SetActive(false);
        }
    }
}
