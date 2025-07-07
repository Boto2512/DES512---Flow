using UnityEngine;
using static Globals;

public static class Utility {
    #region LayerMask Logic
    public static bool DoesMaskContainLayer(LayerMask mask, int layer) => (mask & LayerToLayerMask(layer)) != 0;
    public static bool IsSticky(int layer) => DoesMaskContainLayer(STICKY_MASK, layer);
    public static bool IsSticky(string tag) => STICKY_TAGS.Contains(TagHandle.GetExistingTag(tag));
    public static bool IsSticky(TagHandle tag) => STICKY_TAGS.Contains(tag);
    public static bool IsObstacle(int layer) => DoesMaskContainLayer(OBSTACLE_MASK, layer);
    public static LayerMask LayerToLayerMask(int layer) => 0b1 << layer;
    #endregion LayerMask Logic

    #region Vector Logic

    public static Vector3 Horizontal(this Vector3 v) => new(v.x, 0f, v.z);
    public static Vector3 Vertical(this Vector3 v) => new(0f, v.y, 0f);

    public static float HorizontalSqrMagnitude(this Vector3 v) => v.x * v.x + v.z * v.z;
    public static float HorizontalMagnitude(this Vector3 v) => Mathf.Sqrt(HorizontalSqrMagnitude(v));

    public static Vector2 Flattened(this Vector3 v) => new(v.x, v.z);

    #endregion Vector Logic
}
