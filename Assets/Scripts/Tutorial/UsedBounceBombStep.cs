using UnityEngine;

public class UsedBounceBombStep : ITutorialStep
{
    private Sprite tutorialGif;
    public Sprite GetGifSprite() => tutorialGif;
    public void SetGifSprite(Sprite sprite) => tutorialGif = sprite;
    
    private bool hasUsed = false;

    public void OnStepStart()
    {
        TutorialEvents.OnUsedBomb += HandleUsedBomb;
    }

    public void OnStepComplete()
    {
        TutorialEvents.OnUsedBomb -= HandleUsedBomb;
    }

    private void HandleUsedBomb()
    {
        hasUsed = true;
    }

    public bool Validate() => hasUsed;

    public string GetMessage() =>
        "Right-click to detonate the bomb. Don't worry, it won't harm you — jump on top while detonating to go faster!";
}
