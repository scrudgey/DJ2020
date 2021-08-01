//Unity forum page : https://forum.unity.com/threads/how-to-add-wind-to-custom-trees-in-unity-using-tree-creator-shaders.871126/
using UnityEngine;

public class WindSingleMaterial : MonoBehaviour {
    //WORKS WITH ALL THE NATURE/TREE CREATOR SHADERS

    private MaterialPropertyBlock materialPropertyBlock;
    private Renderer render;
    /*
     * The Vector4 wind value works as :
     *  -> x = wind offset on the x value of the leaves
     *  -> y = *same for y
     *  -> z = *same for z
     *  -> w = Wind force applied to all the values above
    */
    [SerializeField] private Vector4 wind;
    /*
     * Good settings to start with :
     * Leafs : Vector4(0.1f, 0.1f, 0.1f, 0.1f)
     * Trunk : Vector4(0, 0, 0, -0.03f)
     * 
     * Put smaller values if your tree isn't vertex paint
     * 
     * Set a negative value to the trunk, makes it moving with the leaves (leaves must have a positive value)
     * 
     * This MaterialPropertyBlock looks very weird and bad with strong wind forces
    */

    void Start() {
        materialPropertyBlock = new MaterialPropertyBlock();
    }

    void Awake() {
        if (!render) render = GetComponent<Renderer>();
    }

    void Update() {
        //render.SetPropertyBlock() is a MaterialPropertyBlock which works only for the current Tree and affects all the materials of the gameobject

        render.GetPropertyBlock(materialPropertyBlock);
        materialPropertyBlock.SetVector("_Wind", wind);
        render.SetPropertyBlock(materialPropertyBlock);
    }
}