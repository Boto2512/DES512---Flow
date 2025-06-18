using UnityEngine;

public class PlayerMovedStep : ITutorialStep
{
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

    public bool Validate() => hasMoved;

    public string GetMessage() => "Press WASD to move!";
}
