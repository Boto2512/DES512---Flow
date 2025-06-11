using UnityEngine;

public interface IDamageable {
    /// <summary>
    /// Returns the health of the entity attached to the interface
    /// </summary>
    /// <returns>health - float </returns>
    public float GetHealth();
    /// <summary>
    /// Used to damage the entity attached to the interface
    /// </summary>
    /// <param name="value"> the amount of damage to inflict </param>
    public void TakeDamage(float value);

    /// <summary>
    /// Used to kill the entity attached to the interface
    /// note: do not use on player
    /// </summary>
    public void Kill();
}
