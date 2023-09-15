using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "ScriptableObjects/CyberRandomizerTemplate")]
public class CyberRandomizerTemplate : ScriptableObject {
    public string cyberName;
    public Color color;

    [Header("datastore")]
    public List<PayData> payDatas;
}