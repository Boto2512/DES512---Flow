using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSceneManager : MonoBehaviour {
    public static LevelSceneManager Instance { get; private set; }

    [SerializeField] private string tutorialSceneName = "Tutorial";
    [SerializeField] private string transitionSceneName = "Transition";

    [SerializeField] private float minimumTransitionTime = 5f;

    private Scene transitionScene;

    private void Awake() {
        DontDestroyOnLoad(this.gameObject);
        DontDestroyOnLoad(Globals.PLAYER.gameObject);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        transitionScene = LoadTransition();
    }

    public async void ChangeLevel(string newLevelName) {
        Scene oldScene = SceneManager.GetActiveScene();

        await EnterTransition();
        SceneManager.SetActiveScene(transitionScene);
        _ = SceneManager.UnloadSceneAsync(oldScene);

        var loadOperation = StartLoadingScene(newLevelName);
        loadOperation.allowSceneActivation = false;
        Task waitForMinimumTime = Task.Delay((int)(minimumTransitionTime * 1000));

        // makes sure that at least 'waitForMinimumTime' has passed
        await Task.WhenAll(waitForMinimumTime, WaitForSceneLoad(loadOperation));

        await ExitTransition();
        loadOperation.allowSceneActivation = true;
    }

    private async Task WaitForSceneLoad(AsyncOperation asyncOp) {
        while (!asyncOp.isDone && asyncOp.progress < 0.9f) {
            await Task.Yield();
        }
    }

    private void LoadTutorial() {

    }

    private Scene LoadTransition() {
        return SceneManager.LoadScene(transitionSceneName, new LoadSceneParameters(LoadSceneMode.Additive, LocalPhysicsMode.Physics3D));
    }

    private AsyncOperation StartLoadingScene(string sceneName) {
        return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
    }

    private async Task EnterTransition() {
        await Task.Yield();
    }

    private async Task ExitTransition() {
        await Task.Yield();
    }
}
