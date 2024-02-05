using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class EscapeMenuKeyController : MonoBehaviour {
    public GameObject keyEntryPrefab;
    public Transform keyEntryContainer;
    public GraphIconReference graphIconReference;
    [Header("view")]
    public TextMeshProUGUI viewTitle;
    public Image viewIcon;
    public TextMeshProUGUI viewDescription;
    public void Initialize(LevelState state) {
        ClearView();
        List<KeyData> physicalKeys = state.delta.physicalKeys.Select(idn => new KeyData(KeyType.physical, idn)).ToList();
        List<KeyData> keycardKeys = state.delta.keycards.Select(idn => new KeyData(KeyType.keycard, idn)).ToList();
        List<KeyData> passwordKeys = state.delta.levelAcquiredPaydata
            .Where(data => data.type == PayData.DataType.password)
            .Select(data => new KeyData(KeyType.password, Random.Range(0, 100))).ToList();
        PopulateKeyColumn(physicalKeys.Concat(keycardKeys).Concat(passwordKeys).ToList());
    }

    void PopulateKeyColumn(List<KeyData> keyDatas) {
        foreach (Transform child in keyEntryContainer) {
            Destroy(child.gameObject);
        }
        bool selectedInitialValue = false;
        foreach (KeyData keyData in keyDatas) {
            GameObject obj = GameObject.Instantiate(keyEntryPrefab);

            KeyEntry entry = obj.GetComponent<KeyEntry>();
            entry.Configure(ClickCallback, keyData);

            obj.transform.SetParent(keyEntryContainer, false);

            if (!selectedInitialValue) {
                selectedInitialValue = true;
                ClickCallback(entry.data);
                Button button = obj.GetComponent<Button>();
                button.Select();
            }
        }
    }

    void ClickCallback(KeyData data) {
        PopulateView(data);
    }

    void PopulateView(KeyData data) {
        viewIcon.enabled = true;
        viewIcon.sprite = data.type switch {
            KeyType.keycard => graphIconReference.keyCard,
            KeyType.physical => graphIconReference.physicalKey,
            KeyType.password => graphIconReference.password,
            _ => graphIconReference.keyCard
        };
        viewTitle.text = $"{data.idn}";
        viewDescription.text = $"{data.type}";
    }

    void ClearView() {
        viewIcon.enabled = false;
        viewTitle.text = $"";
        viewDescription.text = $"";
    }

}
