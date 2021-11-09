using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageableMesh : IDamageable {
    Mesh mesh;
    Vector3[] vertices;
    int[] triangles;
    bool[] hits;
    PrefabPool damagePool;
    private void Start() {
        // Debug.Log(transform.parent.gameObject);
        mesh = transform.GetComponent<MeshFilter>().mesh;
        vertices = mesh.vertices;
        triangles = mesh.triangles;
        hits = new bool[triangles.Length * 3];
        for (int i = 0; i < hits.Length; i++) {
            hits[i] = false;
        }
        damagePool = PoolManager.I.RegisterPool("prefabs/damageDecal");
        RegisterDamageCallback<BulletDamage>(TakeBulletDamage);
    }
    protected DamageResult TakeBulletDamage(BulletDamage damage) {
        if (damage.hit.triangleIndex == -1 || hits[damage.hit.triangleIndex]) {
            return new DamageResult();
        } else {
            hits[damage.hit.triangleIndex] = true;
        }
        int vertIndex1 = triangles[damage.hit.triangleIndex * 3 + 0];
        int vertIndex2 = triangles[damage.hit.triangleIndex * 3 + 1];
        int vertIndex3 = triangles[damage.hit.triangleIndex * 3 + 2];

        Vector3 origin = Toolbox.GetVertexWorldPosition(vertices[vertIndex1], damage.hit.transform);

        GameObject decal = damagePool.GetObject(origin);
        decal.transform.SetParent(transform, true);

        MeshFilter meshFilter = decal.GetComponent<MeshFilter>();

        Mesh mesh = new Mesh();

        Vector3[] vs = new Vector3[4]{
            Toolbox.GetVertexWorldPosition(vertices[vertIndex1], damage.hit.transform) - origin,
            Toolbox.GetVertexWorldPosition(vertices[vertIndex2], damage.hit.transform) - origin,
            Toolbox.GetVertexWorldPosition(vertices[vertIndex3], damage.hit.transform) - origin,
            Toolbox.GetVertexWorldPosition(vertices[vertIndex1], damage.hit.transform) - origin,
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

        mesh.uv = uv;
        // mesh.SetUVs(1, uv);

        meshFilter.mesh = mesh;
        return new DamageResult();
    }
}
