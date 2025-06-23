using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    private List<ITutorialStep> tutorialSteps;
    private int currentStepIndex = 0;

    private bool isTutorialRunning = false;

    private bool stepCompleted = false;

    [SerializeField] private Sprite moveStepGif;
    [SerializeField] private Sprite ReachedBounceBombStepGif;
    [SerializeField] private Sprite UsedBounceBombStepGif;
    [SerializeField] private Sprite EnemyKilledStepGif;
    [SerializeField] private Sprite EnemyFastKillStepGif;
    [SerializeField] private Sprite BarrelExplodedStepGif;

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

        tutorialSteps[0].SetGifSprite(moveStepGif);
        tutorialSteps[1].SetGifSprite(ReachedBounceBombStepGif);
        tutorialSteps[2].SetGifSprite(UsedBounceBombStepGif);
        tutorialSteps[3].SetGifSprite(EnemyKilledStepGif);
        tutorialSteps[4].SetGifSprite(EnemyFastKillStepGif);
        tutorialSteps[5].SetGifSprite(BarrelExplodedStepGif);
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
        ITutorialStep step = tutorialSteps[currentStepIndex];
        string message = step.GetMessage();
        Sprite gif = step.GetGifSprite();
        ToasterManager.Instance.ShowToaster(message, gif);
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

public bool IsCurrentStep(int stepIndex)
{
    return currentStepIndex == stepIndex;
}



    private void EndTutorial()
    {
        isTutorialRunning = false;
        ToasterManager.Instance.ShowToaster("Tutorial complete! Good luck!");
    }
}