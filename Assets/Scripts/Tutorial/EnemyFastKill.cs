using UnityEngine;

public class EnemyFastKill : ITutorialStep
{
    private bool enemyKilledVeryFast = false;

    public EnemyFastKill()
    {
        TutorialEvents.enemyKilledVeryFast += HandleEnemyKilledFast;
    }

    public string GetMessage()
    {
        return "Speed is damage, increase your speed and hit the enemy to kill!";
    }

    public bool Validate()
    {
        return enemyKilledVeryFast;
    }

    private void HandleEnemyKilledFast()
    {
        enemyKilledVeryFast = true;
        TutorialEvents.enemyKilledVeryFast -= HandleEnemyKilledFast;
    }
}