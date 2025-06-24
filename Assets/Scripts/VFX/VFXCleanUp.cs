using UnityEngine;
using UnityEngine.VFX;

public class VFXCleanUp : MonoBehaviour
{
    [SerializeField] private VisualEffect vfx;
    [SerializeField] private float timerTotal;
    bool isTimer;
    float timer;
    // Update is called once per frame
    void Update()
    {
        if (isTimer) { timer += Time.deltaTime; }
        if (timer > timerTotal) {Destroy(gameObject); }
    }

    public void StartTimer() {
        isTimer = true;
    }
}
