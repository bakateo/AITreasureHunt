using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class ProceduralProgressRingMesh : MonoBehaviour
{
    [Header("Shape")]
    public float outerRadius = 0.35f;
    public float innerRadius = 0.27f;
    public int segments = 96;

    [Header("Progress")]
    [Range(0f, 1f)]
    public float progress = 0f;

    [Header("Direction")]
    public bool clockwise = true;

    [Tooltip("90 = Start oben, 0 = Start rechts, 180 = Start links")]
    public float startAngleDegrees = 90f;

    private Mesh mesh;
    private float lastProgress = -1f;

    private void Awake()
    {
        mesh = new Mesh();
        mesh.name = "Procedural Progress Ring Mesh";

        MeshFilter meshFilter = GetComponent<MeshFilter>();
        meshFilter.sharedMesh = mesh;

        RebuildMesh();
    }

    private void Update()
    {
        if (Mathf.Abs(progress - lastProgress) > 0.001f)
        {
            RebuildMesh();
        }
    }

    public void SetProgress(float value)
    {
        progress = Mathf.Clamp01(value);
        RebuildMesh();
    }

    private void RebuildMesh()
    {
        if (mesh == null)
        {
            mesh = new Mesh();
            mesh.name = "Procedural Progress Ring Mesh";
            GetComponent<MeshFilter>().sharedMesh = mesh;
        }

        lastProgress = progress;

        if (segments < 8)
        {
            segments = 8;
        }

        mesh.Clear();

        if (progress <= 0.001f)
        {
            return;
        }

        int activeSegments = Mathf.CeilToInt(segments * progress);
        activeSegments = Mathf.Clamp(activeSegments, 1, segments);

        Vector3[] vertices = new Vector3[(activeSegments + 1) * 2];
        int[] triangles = new int[activeSegments * 6];

        float directionMultiplier = clockwise ? -1f : 1f;
        float startAngleRad = startAngleDegrees * Mathf.Deg2Rad;

        for (int i = 0; i <= activeSegments; i++)
        {
            float t = (float)i / segments;
            float angle = startAngleRad + directionMultiplier * t * Mathf.PI * 2f;

            float x = Mathf.Cos(angle);
            float y = Mathf.Sin(angle);

            vertices[i * 2] = new Vector3(x * outerRadius, y * outerRadius, 0f);
            vertices[i * 2 + 1] = new Vector3(x * innerRadius, y * innerRadius, 0f);
        }

        for (int i = 0; i < activeSegments; i++)
        {
            int outerCurrent = i * 2;
            int innerCurrent = i * 2 + 1;
            int outerNext = (i + 1) * 2;
            int innerNext = (i + 1) * 2 + 1;

            int triIndex = i * 6;

            triangles[triIndex] = outerCurrent;
            triangles[triIndex + 1] = innerCurrent;
            triangles[triIndex + 2] = outerNext;

            triangles[triIndex + 3] = innerCurrent;
            triangles[triIndex + 4] = innerNext;
            triangles[triIndex + 5] = outerNext;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }
}