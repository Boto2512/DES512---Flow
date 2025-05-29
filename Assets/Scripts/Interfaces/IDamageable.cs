using UnityEngine;

public interface IDamageable
{
    public int GetHealth();
    public int TakeDamage(int value);
}
