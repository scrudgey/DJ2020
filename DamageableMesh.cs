using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageableMesh : MonoBehaviour {
    Mesh mesh;
    Vector3[] vertices;
    int[] triangles;
    private void Start() {
        mesh = transform.GetComponent<MeshFilter>().mesh;
        vertices = mesh.vertices;
        triangles = mesh.triangles;
    }

    public void OnImpact(RaycastHit hit) {
        // Ray ray = Camera.main.ScreenPointToRay(coordinates);

        // if (Physics.Raycast(ray, out RaycastHit hit)) {

        int vertIndex1 = triangles[hit.triangleIndex * 3 + 0];
        int vertIndex2 = triangles[hit.triangleIndex * 3 + 1];
        int vertIndex3 = triangles[hit.triangleIndex * 3 + 2];


        Vector3 origin = Toolbox.GetVertexWorldPosition(vertices[vertIndex1], hit.transform);

        GameObject decal = PoolManager.I.damageDecalPool.SpawnDecal(origin);

        MeshFilter meshFilter = decal.GetComponent<MeshFilter>();

        Mesh mesh = new Mesh();

        Vector3[] vs = new Vector3[4]
        {
            Toolbox.GetVertexWorldPosition(vertices[vertIndex1], hit.transform) - origin,
            Toolbox.GetVertexWorldPosition(vertices[vertIndex2], hit.transform) - origin,
            Toolbox.GetVertexWorldPosition(vertices[vertIndex3], hit.transform) - origin,
            Toolbox.GetVertexWorldPosition(vertices[vertIndex1], hit.transform) - origin,
        };
        mesh.vertices = vs;

        int[] tris = new int[6]
        {
            // lower left triangle
            0, 2, 1,
            // upper right triangle
            2, 3, 1
        };
        mesh.triangles = tris;

        Vector3[] normals = new Vector3[4]
        {
            -Vector3.forward,
            -Vector3.forward,
            -Vector3.forward,
            -Vector3.forward
        };
        mesh.normals = normals;

        Vector2[] uv = new Vector2[4]
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(1, 1)
        };

        // Vector2[] uv = new Vector2[3]
        // {
        //     new Vector2(0, 0),
        //     new Vector2(1, 0),
        //     new Vector2(0, 1),
        //     // new Vector2(1, 1)
        // };

        //         Vector2[] uv = new Vector2[4]
        // {
        //             new Vector2(0, 0),
        //             new Vector2(1, 0),
        //             new Vector2(1, 1),
        //             new Vector2(1, 1)
        // };
        mesh.uv = uv;
        // mesh.SetUVs(1, uv);

        meshFilter.mesh = mesh;

        // } else {
        //     Debug.Log("no hit");
        // }
    }
}
