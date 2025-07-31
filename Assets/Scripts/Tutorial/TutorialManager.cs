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
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        InitializeTutorialSteps();
        StartTutorial();
    }

    private void Update()
    {
        if (!isTutorialRunning || stepCompleted)
            return;

        ITutorialStep currentStep = tutorialSteps[currentStepIndex];

        if (currentStep.Validate() && !stepCompleted)
        {
            stepCompleted = true;

            if (currentStep.ShouldAutoAdvance())
            {
                NextStep();
                stepCompleted = false;
            }
        }
    }

    /// <summary>
    /// External trigger to proceed to the next step (e.g. from UI).
    /// </summary>
    public void NotifyStepConfirmed()
    {
        if (!stepCompleted) return;

        NextStep();
        stepCompleted = false;
    }

    /// <summary>
    /// Initializes all tutorial steps in sequence.
    /// </summary>
    private void InitializeTutorialSteps()
    {
        tutorialSteps = new List<ITutorialStep>
        {
            new PlayerMovedStep(),
            new ReachedBounceBombStep(),
            new UsedBounceBombStep(),
            new EnemyKilledStep(),
            new EnemyFastKill(),
            new BarrelExplodedStep(),
            new LevelExitStep()
        };
    }

    /// <summary>
    /// Begins the tutorial.
    /// </summary>
    public void StartTutorial()
    {
        isTutorialRunning = true;
        tutorialSteps[currentStepIndex].OnStepStart();
        StartCoroutine(DelayedShowMessage());
    }

    private System.Collections.IEnumerator DelayedShowMessage()
    {
        yield return null; // Wait 1 frame to ensure UI is ready
        ShowCurrentStepMessage();
    }

    /// <summary>
    /// Displays the message for the current step, and triggers the associated animation.
    /// </summary>
    private void ShowCurrentStepMessage()
    {
        if (currentStepIndex < tutorialSteps.Count)
        {
            ITutorialStep step = tutorialSteps[currentStepIndex];
            string message = step.GetMessage();
            string animationTrigger = step.GetAnimationTrigger();

            ToasterManager.Instance.ShowToaster(message, animationTrigger);
        }
        else
        {
            EndTutorial();
        }
    }

    /// <summary>
    /// Advances to the next step in the tutorial.
    /// </summary>
    private void NextStep()
    {
        tutorialSteps[currentStepIndex].OnStepComplete();

        currentStepIndex++;

        if (currentStepIndex < tutorialSteps.Count)
        {
            tutorialSteps[currentStepIndex].OnStepStart();
            ShowCurrentStepMessage();
        }
        else
        {
            EndTutorial();
        }
    }

    /// <summary>
    /// Ends the tutorial sequence.
    /// </summary>
    private void EndTutorial()
    {
        isTutorialRunning = false;
        ToasterManager.Instance.ShowToaster("Tutorial complete! Good luck!");
    }

    /// <summary>
    /// Resets the tutorial and starts over from the first step.
    /// </summary>
    public void ResetTutorial()
    {
        if (isTutorialRunning && currentStepIndex < tutorialSteps.Count)
        {
            tutorialSteps[currentStepIndex].OnStepComplete();
        }

        StopAllCoroutines();

        currentStepIndex = 0;
        stepCompleted = false;
        isTutorialRunning = false;

        InitializeTutorialSteps();
        StartTutorial();
    }

    /// <summary>
    /// Used to check from outside whether the current step matches.
    /// </summary>
    public bool IsCurrentStep(int stepIndex)
    {
        return currentStepIndex == stepIndex;
    }
}
