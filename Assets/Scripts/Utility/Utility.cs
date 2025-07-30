using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using static Globals;

public static class Utility {
    #region LayerMask Logic
    public static bool DoesMaskContainLayer(LayerMask mask, int layer) => (mask & LayerToLayerMask(layer)) != 0;
    public static bool IsSticky(int layer) => DoesMaskContainLayer(STICKY_MASK, layer);
    public static bool IsObstacle(int layer) => DoesMaskContainLayer(OBSTACLE_MASK, layer);
    public static LayerMask LayerToLayerMask(int layer) => 0b1 << layer;
    #endregion LayerMask Logic

    #region Vector Logic

    public static Vector3 Horizontal(this Vector3 v) => new(v.x, 0f, v.z);
    public static Vector3 Vertical(this Vector3 v) => new(0f, v.y, 0f);

    public static float HorizontalSqrMagnitude(this Vector3 v) => v.x * v.x + v.z * v.z;
    public static float HorizontalMagnitude(this Vector3 v) => Mathf.Sqrt(HorizontalSqrMagnitude(v));

    public static Vector2 Flattened(this Vector3 v) => new(v.x, v.z);

    /// <summary>
    /// Creates a copy of the Vector3 with different named values
    /// </summary>
    public static Vector3 With(this Vector3 v, float? x, float? y, float? z) {
        return new Vector3(
            x ?? v.x,
            y ?? v.y,
            z ?? v.z
            );
    }

    #endregion Vector Logic

    #region UniTask Logic

    public static async UniTaskVoid RunNextFrame(Action lambda) {
        await UniTask.NextFrame();
        lambda?.Invoke();
    }

    #endregion UniTask Logic

    #region Maths

    /// <summary>
    /// Clamps the value and returns true if the value actually was clamped
    /// </summary>
    public static bool TryClamp(ref float value, float min, float max) {
        if (min > max)
            throw new Exception("TryClamp min param cannot be larger than max param");

        bool clamped = false;

        if (value < min) {
            value = min;
            clamped = true;
        }
        else if (value > max) {
            value = max;
            clamped = true;
        }

        return clamped;
    }

    #endregion Maths

    #region Mesh Logic

    /// <summary>
    /// Warning: This method is quite resource-heavy. Do not use often.
    /// </summary>
    public static Vector3 ClosestPointOnConcaveMesh(this MeshCollider mesh, Vector3 worldPoint) => ClosestPointOnConcaveMesh(mesh, worldPoint, out _);

    /// <summary>
    /// Warning: This method is quite resource-heavy. Do not use often.
    /// </summary>
    public static Vector3 ClosestPointOnConcaveMesh(this MeshCollider mesh, Vector3 worldPoint, out Vector3 normal) {
        Vector3 localOther = mesh.transform.InverseTransformPoint(worldPoint);
        Vector3[] vertices = mesh.sharedMesh.vertices;
        int[] triangles = mesh.sharedMesh.triangles;

        int chosenTriangle = 0;
        Vector3 closestPointLocal = localOther;
        float minDistanceSqr = float.MaxValue;
        for (int i = 0; i < triangles.Length; i += 3) {
            Vector3 vertex1 = vertices[triangles[i]];
            Vector3 vertex2 = vertices[triangles[i + 1]];
            Vector3 vertex3 = vertices[triangles[i + 2]];

            Vector3 currentClosestPoint = ClosestPointOnTriangle(localOther, vertex1, vertex2, vertex3);
            float currentDistanceSqr = (localOther - currentClosestPoint).sqrMagnitude;

            if (currentDistanceSqr <= minDistanceSqr) {
                minDistanceSqr = currentDistanceSqr;
                closestPointLocal = currentClosestPoint;
                chosenTriangle = i;
            }
        }

        normal = new Plane(vertices[triangles[chosenTriangle]], vertices[triangles[chosenTriangle + 1]], vertices[triangles[chosenTriangle + 2]]).normal;
        return mesh.transform.TransformPoint(closestPointLocal);
    }

    public static Vector3 ClosestPointOnTriangle(Vector3 point, Vector3 a, Vector3 b, Vector3 c) {
        // Compute edges
        Vector3 ab = b - a;
        Vector3 ac = c - a;
        Vector3 ap = point - a;

        float d1 = Vector3.Dot(ab, ap);
        float d2 = Vector3.Dot(ac, ap);

        // Corner region of A
        if (d1 <= 0f && d2 <= 0f) return a;

        // Check if P in vertex region outside B
        Vector3 bp = point - b;
        float d3 = Vector3.Dot(ab, bp);
        float d4 = Vector3.Dot(ac, bp);
        if (d3 >= 0f && d4 <= d3) return b;

        // Check if P in edge region of AB
        float vc = d1 * d4 - d3 * d2;
        if (vc <= 0f && d1 >= 0f && d3 <= 0f) {
            float v = d1 / (d1 - d3);
            return a + v * ab;
        }

        // Check if P in vertex region outside C
        Vector3 cp = point - c;
        float d5 = Vector3.Dot(ab, cp);
        float d6 = Vector3.Dot(ac, cp);
        if (d6 >= 0f && d5 <= d6) return c;

        // Check if P in edge region of AC
        float vb = d5 * d2 - d1 * d6;
        if (vb <= 0f && d2 >= 0f && d6 <= 0f) {
            float w = d2 / (d2 - d6);
            return a + w * ac;
        }

        // Check if P in edge region of BC
        float va = d3 * d6 - d5 * d4;
        if (va <= 0f && (d4 - d3) >= 0f && (d5 - d6) >= 0f) {
            float w = (d4 - d3) / ((d4 - d3) + (d5 - d6));
            return b + w * (c - b);
        }

        // P inside face region. Compute Q through barycentric coordinates (u,v,w)
        float denom = 1f / (va + vb + vc);
        float v2 = vb * denom;
        float w2 = vc * denom;
        return a + ab * v2 + ac * w2;
    }

    #endregion Mesh Logic
}
