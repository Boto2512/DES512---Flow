using UnityEngine;

public class PlayerMovedStep : ITutorialStep
{
    private bool hasMoved = false;

    public PlayerMovedStep()
    {
        // Listen for the event
        TutorialEvents.OnPlayerMoved += () => hasMoved = true;
    }

    public bool Validate() => hasMoved;
    public string GetMessage() => "Press WASD to move and Space for jump!";
}
