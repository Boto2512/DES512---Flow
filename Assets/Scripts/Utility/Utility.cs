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
}
