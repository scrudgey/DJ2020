using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "ScriptableObjects/Cyberdeck")]
public class CyberdeckTemplate : ScriptableObject {
    public Sprite art;
    public string title;
    [TextArea(15, 20)]
    public string description;
    public List<SoftwareScriptableTemplate> intrinsicSoftware;
    [Header("stats")]
    public int softwareSlots;
    public int storageCapacity;
    [Header("u/d")]
    public float uploadSpeed;
    public float downloadSpeed;
}