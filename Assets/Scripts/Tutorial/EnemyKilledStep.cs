using UnityEngine;

public class EnemyKilledStep : ITutorialStep
{
    private bool enemyKilled = false;

    public EnemyKilledStep()
    {
        TutorialEvents.OnEnemyKilled += HandleEnemyKilled;
    }

    public string GetMessage()
    {
        return "Hit the enemy with left click!";
    }

    public bool Validate()
    {
        return enemyKilled;
    }

    private void HandleEnemyKilled()
    {
        enemyKilled = true;
        TutorialEvents.OnEnemyKilled -= HandleEnemyKilled;
    }
}