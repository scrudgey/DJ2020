using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "ScriptableObjects/MeleeWeaponTemplate")]
public class MeleeWeaponTemplate : ScriptableObject {
    new public string name;
    public Sprite sprite;

    public AudioClip[] swordUnholsterSound;
    public AudioClip[] swordHolsterSound;
    public AudioClip[] swordSwingSound;
    public AudioClip[] swordImpactNormalSound;
    public AudioClip[] swordImpactHurtableSound;
    public static MeleeWeaponTemplate Load(string name) {
        return Resources.Load($"data/guns/{name}") as MeleeWeaponTemplate;
    }
}
