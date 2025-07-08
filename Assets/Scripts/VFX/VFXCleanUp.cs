using UnityEngine;
using UnityEngine.VFX;

public class VFXCleanUp : MonoBehaviour
{
    [SerializeField] private float timerTotal;
    bool isTimer;
    float timer;
    void Update()
    {
        if (isTimer) { timer += Time.deltaTime; }
        if (timer > timerTotal) {Destroy(gameObject); }
    }

    public void StartTimer() {
        isTimer = true;
    }
}
