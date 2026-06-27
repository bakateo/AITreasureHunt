using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class ProceduralRingMesh : MonoBehaviour
{
    public float outerRadius = 0.35f;
    public float innerRadius = 0.27f;
    public int segments = 96;

    private void Awake()
    {
        CreateRing();
    }

    private void OnValidate()
    {
        CreateRing();
    }

    private void CreateRing()
    {
        if (segments < 8)
        {
            segments = 8;
        }

        Mesh mesh = new Mesh();
        mesh.name = "Procedural Ring Mesh";

        Vector3[] vertices = new Vector3[segments * 2];
        int[] triangles = new int[segments * 6];

        for (int i = 0; i < segments; i++)
        {
            float angle = ((float)i / segments) * Mathf.PI * 2f;
            float x = Mathf.Cos(angle);
            float y = Mathf.Sin(angle);

            vertices[i * 2] = new Vector3(x * outerRadius, y * outerRadius, 0f);
            vertices[i * 2 + 1] = new Vector3(x * innerRadius, y * innerRadius, 0f);
        }

        for (int i = 0; i < segments; i++)
        {
            int next = (i + 1) % segments;

            int outerCurrent = i * 2;
            int innerCurrent = i * 2 + 1;
            int outerNext = next * 2;
            int innerNext = next * 2 + 1;

            int triIndex = i * 6;

            triangles[triIndex] = outerCurrent;
            triangles[triIndex + 1] = outerNext;
            triangles[triIndex + 2] = innerCurrent;

            triangles[triIndex + 3] = innerCurrent;
            triangles[triIndex + 4] = outerNext;
            triangles[triIndex + 5] = innerNext;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        MeshFilter meshFilter = GetComponent<MeshFilter>();
        meshFilter.sharedMesh = mesh;
    }
}