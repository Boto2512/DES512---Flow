using UnityEngine;
using UnityEngine.VFX;

public class WarmUpVFX : MonoBehaviour {
    [SerializeField] private VisualEffect[] vfxToWarmUp;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        foreach (var vfx in vfxToWarmUp) {
            vfx.Reinit();
            vfx.Play();
            vfx.Stop();
        }
    }

    // Update is called once per frame
    void Update() {

    }
}
