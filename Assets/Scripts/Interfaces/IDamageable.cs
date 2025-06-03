using UnityEngine;

public interface IDamageable
{
    public float GetHealth();
    public void TakeDamage(float value);

    public void Kill();
}
