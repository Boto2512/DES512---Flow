using UnityEngine;

public class EnemyFastKill : ITutorialStep
{
    public bool ShouldAutoAdvance() => true;
    private Sprite tutorialGif;
    public Sprite GetGifSprite() => tutorialGif;
    public void SetGifSprite(Sprite sprite) => tutorialGif = sprite;
    public string GetAnimationTrigger() => "FastAttacked"; 
    private bool enemyKilledVeryFast = false;

    public void OnStepStart()
    {
        TutorialEvents.enemyKilledVeryFast += HandleEnemyKilledFast;
    }

    public void OnStepComplete()
    {
        TutorialEvents.enemyKilledVeryFast -= HandleEnemyKilledFast;
    }

    private void HandleEnemyKilledFast()
    {
        enemyKilledVeryFast = true;
    }

    public bool Validate()
    {
        return enemyKilledVeryFast;
    }

    public string GetMessage()
    {
        return "Speed is damage — increase your speed and hit the enemy to kill!";
    }
}
