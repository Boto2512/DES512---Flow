using System;
using UnityEngine;

public interface ITutorialStep
{
    void OnStepStart();
    void OnStepComplete();
    bool Validate();
    string GetMessage();
    Sprite GetGifSprite();
    void SetGifSprite(Sprite sprite);

    bool ShouldAutoAdvance() => false;

}

