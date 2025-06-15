using System;
using UnityEngine;

public interface ITutorialStep
{
    bool Validate();           // Returns true when the step is complete
    string GetMessage();       // Message to display for this step
}
