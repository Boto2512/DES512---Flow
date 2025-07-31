using UnityEngine;

public class PlayerMovedStep : ITutorialStep
{
    public bool Validate() => hasMoved;
    public string GetMessage() => "Press WASD to move and SPACE to Jump!";
    private Sprite tutorialGif;
    public Sprite GetGifSprite() => tutorialGif;
    public void SetGifSprite(Sprite sprite) => tutorialGif = sprite;
    public string GetAnimationTrigger() => "Moved"; 

    private bool hasMoved = false;
    

    public void OnStepStart()
    {
        TutorialEvents.OnPlayerMoved += HandleMoved;
    }

    public void OnStepComplete()
    {
        TutorialEvents.OnPlayerMoved -= HandleMoved;
    }

    private void HandleMoved()
    {
        hasMoved = true;
    }

    
}
