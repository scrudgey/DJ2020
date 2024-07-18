using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class EscapeMenuKeyController : MonoBehaviour {
    public GameObject keyEntryPrefab;
    public GameObject keyListHeaderPrefab;
    public Transform keyEntryContainer;
    public GraphIconReference graphIconReference;
    [Header("view")]
    public TextMeshProUGUI viewTitle;
    public Image viewIcon;
    public TextMeshProUGUI viewDescription;
    public void Initialize(LevelDelta delta) {
        ClearView();
        List<KeyData> passwordKeys = delta.levelAcquiredPaydata
            .Where(data => data.type == PayData.DataType.password)
            .Select(data => new KeyData(KeyType.password, Random.Range(0, 100))).ToList();
        PopulateKeyColumn(delta.keys.Concat(passwordKeys).ToList());
    }

    void PopulateKeyColumn(List<KeyData> keyDatas) {
        foreach (Transform child in keyEntryContainer) {
            Destroy(child.gameObject);
        }
        bool selectedInitialValue = false;
        Dictionary<KeyType, List<KeyData>> sortedData = new Dictionary<KeyType, List<KeyData>>();
        foreach (KeyData data in keyDatas) {
            if (!sortedData.ContainsKey(data.type)) {
                sortedData[data.type] = new List<KeyData>();
            }
            sortedData[data.type].Add(data);
        }

        foreach (KeyValuePair<KeyType, List<KeyData>> kvp in sortedData) {
            GameObject headerObj = GameObject.Instantiate(keyListHeaderPrefab);
            TextMeshProUGUI headerText = headerObj.GetComponentInChildren<TextMeshProUGUI>();
            headerText.text = kvp.Key.ToString();
            headerObj.transform.SetParent(keyEntryContainer, false);


            foreach (KeyData keyData in kvp.Value) {
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
        viewDescription.text = ReadableKeyDescription(data);
        viewTitle.text = ReadableKeyType(data);
    }

    void ClearView() {
        viewIcon.enabled = false;
        viewTitle.text = $"";
        viewDescription.text = $"";
    }

    public static string ReadableKeyType(KeyData data) => data.type switch {
        KeyType.keycard => $"keycard {data.idn}",
        KeyType.keycardCode => $"keycard code {data.idn}",
        KeyType.keypadCode => $"numeric code {data.idn}",
        KeyType.password => $"password {data.idn}",
        KeyType.physical => $"physical key {data.idn}",
        KeyType.physicalCode => $"key bitting code {data.idn}"
    };

    public static string ReadableKeyDescription(KeyData data) {
        switch (data.type) {
            case KeyType.keycard:
                return $"This is a keycard with stored code {data.idn}";
            case KeyType.keycardCode:
                return $"This is the code to a keycard lock. If you have a keycard flash tool, you can create a card with this code.";
            case KeyType.keypadCode:
                return $"This is a code to be entered on a numeric keypad lock.";
            case KeyType.password:
                return $"This is a password to unlock a cyber node.";
            case KeyType.physical:
                return $"This is a physical key with bitting code {data.idn}.";
            case KeyType.physicalCode:
                return $"This is the bitting code for a physical key. If you have a key cutter tool, you can create a key with this code.";
            default:
                return "I don't know what this is!";
        }
    }

}
