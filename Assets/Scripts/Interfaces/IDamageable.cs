using UnityEngine;

public interface IDamageable
{
    public int GetHealth();
    public void TakeDamage(int value);
}
