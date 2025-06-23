using UnityEngine;

public class ReachedBounceBombStep : ITutorialStep
{
    private Sprite tutorialGif;
    public Sprite GetGifSprite() => tutorialGif;
    public void SetGifSprite(Sprite sprite) => tutorialGif = sprite;
    
    private bool hasReached = false;

    public void OnStepStart()
    {
        TutorialEvents.OnReachedBounceBomb += HandleReached;
    }

    public void OnStepComplete()
    {
        TutorialEvents.OnReachedBounceBomb -= HandleReached;
    }

    private void HandleReached()
    {
        hasReached = true;
        
    }

    public bool Validate() => hasReached;

    public string GetMessage() => "Stand in front of the wall and right-click to throw a Bounce Bomb!";
}
