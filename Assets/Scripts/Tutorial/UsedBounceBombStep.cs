using UnityEngine;

public class UsedBounceBombStep : ITutorialStep
{
    private bool hasUsed = false;

    public UsedBounceBombStep()
    {
        TutorialEvents.OnUsedBomb += () => hasUsed = true;
    }

    public bool Validate() => hasUsed;
    public string GetMessage() => "Right click to blow up the bomb and use it to jump!";
}
