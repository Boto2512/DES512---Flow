using UnityEngine;

public class BarrelExplodedStep : ITutorialStep
{
    public bool ShouldAutoAdvance() => true;
    private Sprite tutorialGif;
    public Sprite GetGifSprite() => tutorialGif;
    public void SetGifSprite(Sprite sprite) => tutorialGif = sprite;
    
    private bool barrelExploded = false;

    public void OnStepStart()
    {
        TutorialEvents.OnBarrelExploded += HandleBarrelExploded;
    }

    public void OnStepComplete()
    {
        TutorialEvents.OnBarrelExploded -= HandleBarrelExploded;
    }

    private void HandleBarrelExploded()
    {
        barrelExploded = true;
        TutorialEvents.OnLevelExit?.Invoke();
    }

    public bool Validate() => barrelExploded;

    public string GetMessage() => "Explode the barrel using Bomb Bounce!";
}
