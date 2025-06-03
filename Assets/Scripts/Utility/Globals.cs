using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public static class Globals {
    #region Layers
    public static readonly LayerMask DEFAULT_LAYERMASK = Utility.LayerToLayerMask(LayerMask.NameToLayer("Default"));
    public static readonly LayerMask GROUND_LAYERMASK = Utility.LayerToLayerMask(LayerMask.NameToLayer("Ground"));
        
    public static readonly LayerMask STICKY_MASK = DEFAULT_LAYERMASK | GROUND_LAYERMASK;
    public static readonly HashSet<string> STICKY_TAGS = new() { };
    #endregion Layers

    private static GameObject player;
    /// <summary>
    /// Invokes EVENT_PLAYER_MODIFIED when PLAYER is set
    /// </summary>
    public static GameObject PLAYER {
        get { return player; }
        set {
            player = value;
            EVENT_PLAYER_MODIFIED.Invoke();
        }
    }
    public static readonly UnityEvent EVENT_PLAYER_MODIFIED = new();

    public static readonly TagHandle PLAYER_TAG = TagHandle.GetExistingTag("Player");
}
