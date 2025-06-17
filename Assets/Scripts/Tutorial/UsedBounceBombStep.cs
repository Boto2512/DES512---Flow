using UnityEngine;

public class UsedBounceBombStep : ITutorialStep
{
    private bool hasUsed = false;

    public UsedBounceBombStep()
    {
        TutorialEvents.OnUsedBomb += () => hasUsed = true;
    }

    public bool Validate() => hasUsed;
    public string GetMessage() => "Right click to detonate the bomb. Don't worry it won't harm you, jump on top while detonating to go faster!";
}
