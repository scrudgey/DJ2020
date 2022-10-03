using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class GunStatHandler : MonoBehaviour {
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI typeText;
    public TextMeshProUGUI cycleText;
    public TextMeshProUGUI shootIntervalText;
    public TextMeshProUGUI noiseText;
    public TextMeshProUGUI clipSizeText;
    public TextMeshProUGUI spreadText;
    public TextMeshProUGUI recoilText;
    public TextMeshProUGUI lockSizeText;
    public TextMeshProUGUI damageLowText;
    public TextMeshProUGUI damageHighText;
    public TextMeshProUGUI weightText;
    public TextMeshProUGUI recoilLowText;
    public TextMeshProUGUI recoilHighText;
    public Image gunImage;
    public void DisplayGunTemplate(GunTemplate template) {
        nameText.text = template.name;
        typeText.text = template.type.ToString();
        cycleText.text = template.cycle.ToString();
        shootIntervalText.text = (template.shootInterval * 10).ToString();
        noiseText.text = template.noise.ToString();
        clipSizeText.text = template.clipSize.ToString();
        spreadText.text = (template.spread * 10).ToString();
        recoilText.text = template.shootInaccuracy.ToString();
        lockSizeText.text = template.lockOnSize.ToString() + "m";
        damageLowText.text = template.baseDamage.low.ToString();
        damageHighText.text = template.baseDamage.high.ToString();
        weightText.text = template.weight.ToString();
        recoilLowText.text = (template.recoil.low * 10).ToString();
        recoilHighText.text = (template.recoil.high * 10).ToString();
        gunImage.sprite = template.image;
    }
}
