using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ResourceEntry {
    public UnityEngine.Object key;
    public string value;
}
public class ResourceReferencePopulator : MonoBehaviour {
    public ResourceReference target;
    public List<UnityEngine.Object> keys;
    public List<string> values;
}
