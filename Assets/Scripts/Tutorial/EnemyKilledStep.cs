using UnityEngine;

public class EnemyKilledStep : ITutorialStep
{
    private Sprite tutorialGif;
    public Sprite GetGifSprite() => tutorialGif;
    public void SetGifSprite(Sprite sprite) => tutorialGif = sprite;
    public string GetAnimationTrigger() => "Attacked"; 
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
