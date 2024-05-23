using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Easings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class MissionSelectSoftwareCraftController : MonoBehaviour {
    public SoftwareTemplate template;
    public MissionComputerController missionComputerController;
    public List<Sprite> availableSprites;
    [Header("main info")]
    public TMP_InputField nameInputField;
    public Image exploitSelectorBackground;
    public Image virustSelectorBackground;
    public SoftwareButton softwareIcon;
    public GameObject virusTypeButton;
    [Header("params")]
    public TextMeshProUGUI paramChargesNumber;
    public TextMeshProUGUI paramChargesCost;
    public TextMeshProUGUI paramHopsNumber;
    public TextMeshProUGUI paramHopsCost;
    public TextMeshProUGUI paramDupNumber;
    public TextMeshProUGUI paramDupCost;
    public GameObject paramHopsObject;
    public GameObject paramDupObject;
    [Header("payload")]
    public Transform payloadContainer;
    public GameObject payloadPrefab;
    [Header("conditional")]
    public Transform conditionalContainer;
    public GameObject conditionalPrefab;
    [Header("buttonbar")]
    public TextMeshProUGUI sizeText;
    public TextMeshProUGUI designPointsText;

    [Header("sub menu")]
    public GameObject selectPayloadDialogObject;
    public SoftwareEffectPicker softwareEffectPicker;

    public Button buildButton;
    public Color validColor;
    public Color invalidColor;
    GameData data;
    bool inEditMode;
    SoftwareTemplate copyOfOriginalTemplate;
    int designPointBudget;
    int sizeOffset;
    bool canCraftViruses = false;
    public void Initialize(GameData data) {
        this.data = data;
        this.template = SoftwareTemplate.Empty();
        designPointBudget = data.playerState.SoftwareDesignPointBudget();
        canCraftViruses = data.playerState.CanCraftViruses();
        sizeOffset = data.playerState.SoftwareDesignSizeBonus();
        template.icon = Toolbox.RandomFromList(availableSprites);
        CloseAddPayloadDialogue();
        Refresh();
    }
    public void Initialize(GameData data, SoftwareTemplate template) {
        this.data = data;
        this.template = template;
        this.copyOfOriginalTemplate = new SoftwareTemplate(template);
        designPointBudget = data.playerState.SoftwareDesignPointBudget();
        canCraftViruses = data.playerState.CanCraftViruses();
        sizeOffset = data.playerState.SoftwareDesignSizeBonus();

        inEditMode = true;
        template.nameHasBeenSet = true;
        CloseAddPayloadDialogue();
        Refresh();
    }

    void Refresh() {
        virusTypeButton.SetActive(canCraftViruses);

        nameInputField.text = template.name;
        if (template.softwareType == SoftwareTemplate.SoftwareType.exploit) {
            exploitSelectorBackground.enabled = true;
            virustSelectorBackground.enabled = false;
            paramHopsObject.SetActive(false);
            paramDupObject.SetActive(false);
        } else if (template.softwareType == SoftwareTemplate.SoftwareType.virus) {
            exploitSelectorBackground.enabled = false;
            virustSelectorBackground.enabled = true;

            paramHopsObject.SetActive(true);
            paramDupObject.SetActive(true);
        }
        softwareIcon.Initialize(template);

        paramChargesNumber.text = $"{template.maxCharges}";
        paramHopsNumber.text = $"{template.virusHops}";
        paramDupNumber.text = $"{template.virusDup}";

        paramChargesCost.text = $"{template.CalculateChargesCost()}";
        paramHopsCost.text = $"{template.CalculateHopsCost()}";
        paramDupCost.text = $"{template.CalculateDupCost()}";

        foreach (Transform child in payloadContainer) {
            Destroy(child.gameObject);
        }
        foreach (SoftwareEffect effect in template.effects) {
            GameObject obj = GameObject.Instantiate(payloadPrefab);
            obj.transform.SetParent(payloadContainer, false);
            PayloadCraftingEntry entry = obj.GetComponent<PayloadCraftingEntry>();
            entry.Initialize(this, effect);
        }

        SetHardRequirements();

        foreach (Transform child in conditionalContainer) {
            Destroy(child.gameObject);
        }
        foreach (SoftwareCondition condition in template.conditions) {
            GameObject obj = GameObject.Instantiate(conditionalPrefab);
            obj.transform.SetParent(conditionalContainer, false);
            SoftwareCraftConditional controller = obj.GetComponent<SoftwareCraftConditional>();
            controller.Initialize(condition);
        }
        designPointsText.text = $"{template.TotalDesignPointsCost()}/{designPointBudget}";
        sizeText.text = $"{template.CalculateSize()} MB";

        ValidateButtons();
    }

    void ValidateButtons() {
        bool designPointsValid = template.TotalDesignPointsCost() <= designPointBudget;
        bool templateNameValid = template.name != "";
        bool valid = designPointsValid;// && templateNameValid;
        if (valid) {
            buildButton.interactable = true;
        } else {
            buildButton.interactable = false;
        }
        designPointsText.color = designPointsValid ? validColor : invalidColor;
    }

    void SetHardRequirements() {
        HashSet<SoftwareCondition> conditions = new HashSet<SoftwareCondition>();

        Dictionary<SoftwareEffect.Type, bool> effects = ((SoftwareEffect.Type[])Enum.GetValues(typeof(SoftwareEffect.Type)))
            .ToDictionary(t => t, t => template.effects.Any(effect => effect.type == t));

        HashSet<SoftwareCondition.Type> conditionsToAdd = new HashSet<SoftwareCondition.Type>();

        if (effects[SoftwareEffect.Type.unlock]) {
            conditionsToAdd.Add(SoftwareCondition.Type.locked);
            conditionsToAdd.Add(SoftwareCondition.Type.nodeKnown);
        }
        if (effects[SoftwareEffect.Type.compromise]) {
            conditionsToAdd.Add(SoftwareCondition.Type.unlocked);
            conditionsToAdd.Add(SoftwareCondition.Type.uncompromised);
        }
        if (effects[SoftwareEffect.Type.scanAll] || effects[SoftwareEffect.Type.scanNode]) {
            conditionsToAdd.Add(SoftwareCondition.Type.nodeUnknown);
        }
        if (effects[SoftwareEffect.Type.scanFile]) {
            conditionsToAdd.Add(SoftwareCondition.Type.nodeType);
            conditionsToAdd.Add(SoftwareCondition.Type.fileUnknown);
        }
        if (effects[SoftwareEffect.Type.scanEdges]) {
            conditionsToAdd.Add(SoftwareCondition.Type.edgesUnknown);
        }

        // remove conditions as indicated
        // if ()
        if (effects[SoftwareEffect.Type.unlock]) {
            conditionsToAdd.Remove(SoftwareCondition.Type.unlocked);
        }
        if (effects[SoftwareEffect.Type.scanAll] || effects[SoftwareEffect.Type.scanNode]) {
            conditionsToAdd.Remove(SoftwareCondition.Type.nodeKnown);
            conditionsToAdd.Remove(SoftwareCondition.Type.nodeType);
        }
        // if (effects[SoftwareEffect.Type.scanFile]) {
        //     conditionsToAdd.Add(SoftwareCondition.Type.nodeType);
        //     conditionsToAdd.Add(SoftwareCondition.Type.fileUnknown);
        // }
        // if (effects[SoftwareEffect.Type.scanEdges]) {
        //     conditionsToAdd.Add(SoftwareCondition.Type.edgesUnknown);
        // }


        if (conditionsToAdd.Contains(SoftwareCondition.Type.locked)) {
            conditions.Add(new SoftwareCondition(SoftwareCondition.Type.locked));
        }
        if (conditionsToAdd.Contains(SoftwareCondition.Type.nodeKnown)) {
            conditions.Add(new SoftwareCondition(SoftwareCondition.Type.nodeKnown));
        }
        if (conditionsToAdd.Contains(SoftwareCondition.Type.unlocked)) {
            conditions.Add(new SoftwareCondition(SoftwareCondition.Type.unlocked));
        }
        if (conditionsToAdd.Contains(SoftwareCondition.Type.uncompromised)) {
            conditions.Add(new SoftwareCondition(SoftwareCondition.Type.uncompromised));
        }
        if (conditionsToAdd.Contains(SoftwareCondition.Type.nodeUnknown)) {
            conditions.Add(new SoftwareCondition(SoftwareCondition.Type.nodeUnknown));
        }
        if (conditionsToAdd.Contains(SoftwareCondition.Type.nodeType)) {
            conditions.Add(new SoftwareCondition(SoftwareCondition.Type.nodeType, CyberNodeType.datanode));
        }
        if (conditionsToAdd.Contains(SoftwareCondition.Type.fileUnknown)) {
            conditions.Add(new SoftwareCondition(SoftwareCondition.Type.fileUnknown));
        }
        if (conditionsToAdd.Contains(SoftwareCondition.Type.edgesUnknown)) {
            conditions.Add(new SoftwareCondition(SoftwareCondition.Type.edgesUnknown));
        }

        template.conditions = conditions.ToList();
    }

    public void CallbackNameChange() {
        template.name = nameInputField.text;
        Refresh();
    }
    public void CallbackTypeExploit() {
        template.softwareType = SoftwareTemplate.SoftwareType.exploit;
        Refresh();
    }
    public void CallbackTypeVirus() {
        template.softwareType = SoftwareTemplate.SoftwareType.virus;
        Refresh();
    }
    public void CallbackParamPlus(string param) {
        switch (param) {
            case "charges":
                template.maxCharges += 1;
                template.maxCharges = Mathf.Min(template.maxCharges, 10);
                break;
            case "hops":
                template.virusHops += 1;
                template.virusHops = Mathf.Min(template.virusHops, 10);
                break;
            case "duplication":
                template.virusDup += 1;
                template.virusDup = Mathf.Min(template.virusDup, 3);
                break;
        }
        Refresh();
    }
    public void CallbackParamMinus(string param) {
        switch (param) {
            case "charges":
                template.maxCharges -= 1;
                template.maxCharges = Mathf.Max(template.maxCharges, 1);
                break;
            case "hops":
                template.virusHops -= 1;
                template.virusHops = Mathf.Max(template.virusHops, 1);
                break;
            case "duplication":
                template.virusDup -= 1;
                template.virusDup = Mathf.Max(template.virusDup, 1);
                break;
        }
        Refresh();
    }
    public void CallbackAddPayload() {
        // open the dialogue
        softwareEffectPicker.Initialize(template);
        selectPayloadDialogObject.SetActive(true);
    }
    public void CloseAddPayloadDialogue() {
        selectPayloadDialogObject.SetActive(false);
    }
    public void AddSoftwareEffect(SoftwareEffect effect) {
        template.effects.Add(effect);
        if (!template.nameHasBeenSet) {
            template.SetRandomName();
        }
        Refresh();
    }
    public void CallbackRemovePayload(PayloadCraftingEntry entry) {
        template.effects.Remove(entry.effect);
        Destroy(entry.gameObject);
        Refresh();
    }
    public void CallbackClear() {
        template = SoftwareTemplate.Empty();
        Refresh();
    }
    public void CallbackCancel() {
        if (inEditMode) {
            // template = copyOfOriginalTemplate;
            data.playerState.softwareTemplates[data.playerState.softwareTemplates.IndexOf(template)] = copyOfOriginalTemplate;
        }
        missionComputerController.ShowSoftwareListView(true);
    }
    public void CallbackBuild() {
        bool templateNameValid = template.name != "";
        if (templateNameValid) {
            if (!inEditMode)
                data.playerState.softwareTemplates.Add(template);
            missionComputerController.ShowSoftwareListView(true);
        } else {
            missionComputerController.ShowModalDialogue($"You must set a valid name for the software!");
        }
    }

    public void ChangeIcon(int increment) {
        int currentIndex = availableSprites.IndexOf(template.icon);
        currentIndex += increment;
        if (currentIndex < 0) {
            currentIndex = availableSprites.Count - 1;
        } else if (currentIndex >= availableSprites.Count) {
            currentIndex = 0;
        }
        template.icon = availableSprites[currentIndex];
        Refresh();
    }

}
