using UnityEngine;
using UnityEngine.VFX;

[RequireComponent(typeof(VisualEffect))]
public class VFXSuicide : MonoBehaviour {

    private VisualEffect vfx;
    private bool hasPlayed = false;

    private void Awake() {
        vfx = this.GetComponent<VisualEffect>();
    }

    void Update() {
        if (!hasPlayed && vfx.aliveParticleCount > 0)
            hasPlayed = true;

        if (vfx.aliveParticleCount == 0 && hasPlayed)
            Destroy(gameObject);
    }
}
