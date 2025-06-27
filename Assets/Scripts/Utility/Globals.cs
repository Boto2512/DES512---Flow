using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public static class Globals {

    #region Layers

    public static readonly LayerMask PLAYER_MASK = Utility.LayerToLayerMask(LayerMask.NameToLayer("Player"));
    public static readonly LayerMask DEFAULT_MASK = Utility.LayerToLayerMask(LayerMask.NameToLayer("Default"));
    public static readonly LayerMask GROUND_MASK = Utility.LayerToLayerMask(LayerMask.NameToLayer("Ground"));
    public static readonly LayerMask PROJECTILE_MASK = Utility.LayerToLayerMask(LayerMask.NameToLayer("Projectile"));
    public static readonly LayerMask ENEMY_MASK = Utility.LayerToLayerMask(LayerMask.NameToLayer("Enemy"));
    public static readonly LayerMask HAZARD_MASK;
    public static readonly LayerMask INTERACTABLE_MASK;

    public static readonly LayerMask OBSTACLE_MASK = DEFAULT_MASK | GROUND_MASK | HAZARD_MASK | INTERACTABLE_MASK;
    public static readonly LayerMask STICKY_MASK = DEFAULT_MASK | GROUND_MASK | HAZARD_MASK | INTERACTABLE_MASK;
    public static readonly HashSet<TagHandle> STICKY_TAGS = new() { };

    #endregion Layers

    #region Maths Constants

    public static readonly float RootTwo = Mathf.Sqrt(2f);

    #endregion

    private static MonoBehaviour player;
    /// <summary>
    /// Invokes EVENT_PLAYER_MODIFIED when PLAYER is set
    /// </summary>
    public static MonoBehaviour PLAYER {
        get { return player; }
        set {
            player = value;
            PLAYER_TARGET = value as ITargetable;
            EVENT_PLAYER_MODIFIED.Invoke();
        }
    }
    public static readonly UnityEvent EVENT_PLAYER_MODIFIED = new();
    public static ITargetable PLAYER_TARGET = null;

    public static readonly TagHandle PLAYER_TAG = TagHandle.GetExistingTag("Player");
}
