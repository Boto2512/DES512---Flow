using UnityEngine;

public interface IDamageable
{
    int Health {  get; set; }
    public int GetHealth();
    public int SetHealth(int value);
}
