using System;
using UnityEngine;

public interface ITutorialStep
{
    void OnStepStart();
    void OnStepComplete();
    bool Validate();
    string GetMessage();
    string GetAnimationTrigger(); // used for toaster animation

    bool ShouldAutoAdvance() => false;
}
