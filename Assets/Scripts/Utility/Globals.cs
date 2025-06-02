using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public static class Globals {
    public static readonly LayerMask DEFAULT_LAYERMASK = Utility.LayerToLayerMask(LayerMask.NameToLayer("Default"));
    public static readonly LayerMask GROUND_LAYERMASK = Utility.LayerToLayerMask(LayerMask.NameToLayer("Ground"));
        
    public static readonly LayerMask STICKY_MASK = DEFAULT_LAYERMASK | GROUND_LAYERMASK;
    public static HashSet<string> STICKY_TAGS = new() { };

    /// <summary>
    /// Invokes EVENT_PLAYER_MODIFIED when PLAYER is set
    /// </summary>
    public static GameObject PLAYER {
        get { return PLAYER; }
        set {
            PLAYER = value;
            EVENT_PLAYER_MODIFIED.Invoke();
        }
    }
    public static UnityEvent EVENT_PLAYER_MODIFIED = new();
}
