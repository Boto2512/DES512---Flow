public class EnemyKilledStep : ITutorialStep
{
    private bool enemyKilled = false;

    public EnemyKilledStep()
    {
        TutorialEvents.OnEnemyKilled += () => enemyKilled = true;
    }

    public bool Validate() => enemyKilled;
    public string GetMessage() => "Hit the enemy with left click!";
}
