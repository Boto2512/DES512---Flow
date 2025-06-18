using UnityEngine;

public class BarrelExplodedStep : ITutorialStep
{
    private bool barrelExploded = false;

    public BarrelExplodedStep()
    {
        TutorialEvents.OnBarrelExploded += () => barrelExploded = true;
    }

    public bool Validate() => barrelExploded;
    public string GetMessage() => "Explode the barrel using Bomb Bounce!";
}
