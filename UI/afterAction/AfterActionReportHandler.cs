using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AfterActionReportHandler : MonoBehaviour {
    public GameObject UIEditorCamera;
    public TextMeshProUGUI missionNameText;
    public Transform objectivesContainer;
    public GameObject objectivePrefab;
    public TextMeshProUGUI emailText;
    public TextMeshProUGUI rewardAmountText;
    public TextMeshProUGUI bonusAmountText;
    public GameObject bonusAmountObject;
    public TextMeshProUGUI balanceAmountText;

    public void Initialize(GameData data) {
        foreach (Transform child in objectivesContainer) {
            if (child.gameObject.name == "divider") continue;
            Destroy(child.gameObject);
        }
        missionNameText.text = data.levelState.template.levelName;
        foreach (Objective objective in data.levelState.template.objectives) {
            GameObject obj = GameObject.Instantiate(objectivePrefab);
            obj.transform.SetParent(objectivesContainer, false);
            AfterActionReportObjectiveHandler handler = obj.GetComponent<AfterActionReportObjectiveHandler>();
            handler.Initialize(objective, data);
        }
        emailText.text = data.levelState.template.email.text;

        rewardAmountText.text = data.levelState.template.creditReward.ToString("n2");
        bonusAmountText.text = "0";
        balanceAmountText.text = data.playerState.credits.ToString("n2");
    }

    public void ContinueButtonCallback() {
        GameManager.I.ReturnToApartment();
    }

}
