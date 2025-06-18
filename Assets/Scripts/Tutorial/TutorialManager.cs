using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    private List<ITutorialStep> tutorialSteps;
    private int currentStepIndex = 0;

    private bool isTutorialRunning = false;

    private bool stepCompleted = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        InitializeTutorialSteps();
        StartTutorial();
    }

    private bool waitingForNextFrame = false;

    private void Update()
    {
        if (!isTutorialRunning || stepCompleted) return;

        ITutorialStep currentStep = tutorialSteps[currentStepIndex];

        if (currentStep.Validate())
        {
            stepCompleted = true;

        }
    }

public void NotifyStepConfirmed()
{
    if (!stepCompleted) return; 

    NextStep();
    stepCompleted = false;  
}

private System.Collections.IEnumerator WaitBeforeNextValidation()
{
    waitingForNextFrame = true;
    yield return null;
    waitingForNextFrame = false;
}


    private void InitializeTutorialSteps()
    {
        tutorialSteps = new List<ITutorialStep>
        {
            new PlayerMovedStep(),
            new ReachedBounceBombStep(),
            new UsedBounceBombStep(),
            new EnemyKilledStep(),
            new EnemyFastKill(),
            new BarrelExplodedStep()
        };
    }

    public void StartTutorial()
{
    isTutorialRunning = true;
    tutorialSteps[currentStepIndex].OnStepStart(); 
    StartCoroutine(DelayedShowMessage());
}


private System.Collections.IEnumerator DelayedShowMessage()
{
    yield return null; // Wait 1 frame
    ShowCurrentStepMessage();
}


    private void ShowCurrentStepMessage()
    {
        if (currentStepIndex < tutorialSteps.Count)
        {
            string message = tutorialSteps[currentStepIndex].GetMessage();
            ToasterManager.Instance.ShowToaster(message);
        }
        else
        {
            EndTutorial();
        }
    }

    private void NextStep()
{
    tutorialSteps[currentStepIndex].OnStepComplete(); // cleanup

    currentStepIndex++;

    if (currentStepIndex < tutorialSteps.Count)
    {
        tutorialSteps[currentStepIndex].OnStepStart(); // setup new
        ShowCurrentStepMessage();
    }
    else
    {
        EndTutorial();
    }
}


    private void EndTutorial()
    {
        isTutorialRunning = false;
        ToasterManager.Instance.ShowToaster("Tutorial complete! Good luck!");
    }
}
