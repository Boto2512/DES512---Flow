using System.Collections.Generic;
using UnityEngine;

public static class Globals {
    public static readonly LayerMask DEFAULT_LAYERMASK = Utility.LayerToLayerMask(LayerMask.NameToLayer("Default"));
    public static readonly LayerMask GROUND_LAYERMASK = Utility.LayerToLayerMask(LayerMask.NameToLayer("Ground"));
        
    public static readonly LayerMask STICKY_MASK = DEFAULT_LAYERMASK | GROUND_LAYERMASK;
    public static HashSet<string> STICKY_TAGS = new() { };
}
