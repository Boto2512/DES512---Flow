using UnityEngine;

public class EnemyKilledStep : ITutorialStep
{
    private bool enemyKilled = false;

    public void OnStepStart()
    {
        TutorialEvents.OnEnemyKilled += HandleEnemyKilled;
    }

    public void OnStepComplete()
    {
        TutorialEvents.OnEnemyKilled -= HandleEnemyKilled;
    }

    private void HandleEnemyKilled()
    {
        enemyKilled = true;
    }

    public bool Validate() => enemyKilled;

    public string GetMessage() => "Hit the enemy with left click!";
}
