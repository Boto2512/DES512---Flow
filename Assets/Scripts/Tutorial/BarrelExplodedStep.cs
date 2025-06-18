using UnityEngine;

public class BarrelExplodedStep : ITutorialStep
{
    private bool barrelExploded = false;

    public void OnStepStart()
    {
        TutorialEvents.OnBarrelExploded += HandleBarrelExploded;
    }

    public void OnStepComplete()
    {
        TutorialEvents.OnBarrelExploded -= HandleBarrelExploded;
    }

    private void HandleBarrelExploded()
    {
        barrelExploded = true;
    }

    public bool Validate() => barrelExploded;

    public string GetMessage() => "Explode the barrel using Bomb Bounce!";
}
