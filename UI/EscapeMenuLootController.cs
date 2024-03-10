using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class EscapeMenuLootController : MonoBehaviour {

    [Header("lists")]
    public GameObject lootButtonPrefab;
    public Transform inventoryContainer;
    public TextMeshProUGUI nothingToSellText;
    [Header("lootpanel")]
    public Image itemImage;
    public LootTypeIcon lootTypeIcon;
    public TextMeshProUGUI lootNameCaption;
    public TextMeshProUGUI valueText;
    public TextMeshProUGUI countText;
    public TextMeshProUGUI totalText;
    public Image[] creditsImages;
    public void Initialize(List<LootData> loot) {
        PopulatePlayerInventory(loot);
    }
    void PopulatePlayerInventory(List<LootData> loot) {
        foreach (Transform child in inventoryContainer) {
            if (child == nothingToSellText.transform) continue;
            Destroy(child.gameObject);
        }
        nothingToSellText.enabled = loot.Count == 0;
        bool selectedInitialValue = false;
        int numberOfitems = 0;
        foreach (IGrouping<string, LootData> grouping in loot.GroupBy(lootData => lootData.lootName)) {
            int count = grouping.Count();
            LootData data = grouping.First();
            GameObject button = CreateLootButton(data);
            LootInventoryButton script = button.GetComponent<LootInventoryButton>();
            script.Initialize(LootButtonCallback, grouping.ToList(), count);
            // Debug.Log($"group: {data.name} {count}");
            button.transform.SetParent(inventoryContainer, false);
            numberOfitems += 1;

            if (!selectedInitialValue) {
                selectedInitialValue = true;
                SetSaleData(grouping.ToList());
                Button uiBitton = button.GetComponent<Button>();
                uiBitton.Select();
            }
        }
    }

    GameObject CreateLootButton(LootData data) {
        GameObject obj = GameObject.Instantiate(lootButtonPrefab);
        return obj;
    }


    public void LootButtonCallback(List<LootData> data) {
        SetSaleData(data);
    }
    void SetSaleData(List<LootData> datas) {
        LootData data = datas[0];
        valueText.text = data.GetValue().ToString();

        itemImage.enabled = true;
        itemImage.sprite = data.portrait;
        lootNameCaption.text = data.lootName;
        lootTypeIcon.SetLootCategory(data.category);
        lootTypeIcon.Show();
        foreach (Image image in creditsImages) {
            image.enabled = true;
        }
        int count = datas.Count();
        countText.text = $"{count}x";

        // float bonus = lootBuyerData.CalculateBonusFactor(data);
        int forSaleTotalPrice = (int)(data.GetValue() * count);
        totalText.text = $"{forSaleTotalPrice}";
        // factorText.text = $"{bonus}x";
    }
    void ClearItemForSale() {
        valueText.text = "-";
        // factorText.text = "-";
        countText.text = "-";
        totalText.text = "-";
        lootNameCaption.text = "";
        itemImage.enabled = false;
        foreach (Image image in creditsImages) {
            image.enabled = false;
        }
        // forSaleTotalPrice = 0;
        lootTypeIcon.Hide();
    }
}
