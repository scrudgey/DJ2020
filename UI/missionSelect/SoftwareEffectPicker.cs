using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class SoftwareEffectPicker : MonoBehaviour {
    public Transform effectPickerContainer;
    public GameObject effectPickerPrefab;
    public MissionSelectSoftwareCraftController missionSelectSoftwareCraftController;
    public PayloadCraftingEntry effectView;
    SoftwareEffect selectedEffect;
    public void Initialize(SoftwareTemplate template) {
        foreach (Transform child in effectPickerContainer) {
            Destroy(child.gameObject);
        }

        // TODO: data driven
        List<SoftwareEffect> effects = new List<SoftwareEffect>() {
            new SoftwareEffect(){ type = SoftwareEffect.Type.scanAll},
            new SoftwareEffect(){ type = SoftwareEffect.Type.unlock},
            new SoftwareEffect(){ type = SoftwareEffect.Type.download},
            new SoftwareEffect(){ type = SoftwareEffect.Type.compromise},
            new SoftwareEffect(){ type = SoftwareEffect.Type.scanNode},
            new SoftwareEffect(){ type = SoftwareEffect.Type.scanEdges},
            new SoftwareEffect(){ type = SoftwareEffect.Type.scanFile}
        };

        selectedEffect = null;
        foreach (SoftwareEffect effect in effects) {
            if (template.effects.Any(e => e.type == effect.type)) continue;
            GameObject obj = GameObject.Instantiate(effectPickerPrefab);
            obj.transform.SetParent(effectPickerContainer, false);
            EffectSelector selector = obj.GetComponent<EffectSelector>();
            selector.Initialize(effect, CallbackEffectSelector);

            if (selectedEffect == null) {
                selectedEffect = effect;
                CallbackEffectSelector(selector);
            }
        }
    }
    public void CallbackEffectSelector(EffectSelector selector) {
        selectedEffect = selector.effect;
        PopulateDisplay(selector.effect);
    }
    void PopulateDisplay(SoftwareEffect effect) {
        effectView.Initialize(null, effect);
    }
    public void CallbackAccept() {
        missionSelectSoftwareCraftController.AddSoftwareEffect(selectedEffect);
        missionSelectSoftwareCraftController.CloseAddPayloadDialogue();
    }
    public void CallbackCancel() {
        // close the thing
        missionSelectSoftwareCraftController.CloseAddPayloadDialogue();
    }
}
