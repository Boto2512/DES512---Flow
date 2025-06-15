using UnityEngine;

public class ReachedBounceBombStep : ITutorialStep
{
    private bool hasReached = false;

    public ReachedBounceBombStep()
    {
        TutorialEvents.OnReachedBounceBomb += () => hasReached = true;
    }

    public bool Validate() => hasReached;
    public string GetMessage() => "Stand in front of the wall and right-click to throw a Bounce Bomb!";
}
