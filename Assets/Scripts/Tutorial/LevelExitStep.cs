using UnityEngine;

public class LevelExitStep : ITutorialStep
{
    public bool ShouldAutoAdvance() => true;
    private Sprite tutorialGif;
    public Sprite GetGifSprite() => tutorialGif;
    public void SetGifSprite(Sprite sprite) => tutorialGif = sprite;

    private bool levelExit = false;

    public void OnStepStart()
    {
        TutorialEvents.OnLevelExit += HandleLevelExit;
    }

    public void OnStepComplete()
    {
        TutorialEvents.OnBarrelExploded -= HandleLevelExit;
    }

    private void HandleLevelExit()
    {
        levelExit = true;
    }

    public bool Validate() => levelExit;

    public string GetMessage() => "Kill all the enemies to activate the steampad and use it to escape!";
}
