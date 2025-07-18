using UnityEngine;
using System;

public class EnemyTracker : MonoBehaviour
{
    public event Action<GameObject> OnEnemyDeath;

    private void OnDestroy()
    {
        OnEnemyDeath?.Invoke(gameObject);
    }
}
