using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PersistentSceneManager : MonoBehaviour {
    public static PersistentSceneManager Instance { get; private set; }

    [SerializeField] private string tutorialSceneName = "Tutorial";
    [SerializeField] private string transitionSceneName = "Transition";

    [SerializeField] private float minimumTransitionTime = 5f;

    private Scene transitionScene;
    public LevelManager CurrentLevelManager;

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
        Scene newScene = SceneManager.GetSceneByName(newLevelName);

        await EnterTransition();

        SceneManager.SetActiveScene(transitionScene);
        _ = SceneManager.UnloadSceneAsync(oldScene);
        MoveScene(transitionScene, Globals.PLAYER.transform);

        var loadOperation = StartLoadingScene(newLevelName);
        loadOperation.allowSceneActivation = false;
        UniTask waitForMinimumTime = UniTask.Delay((int)(minimumTransitionTime * 1000));

        // makes sure that at least 'waitForMinimumTime' has passed
        await UniTask.WhenAll(waitForMinimumTime, WaitForSceneLoad(loadOperation));

        await ExitTransition();
        loadOperation.allowSceneActivation = true;
        await loadOperation.ToUniTask();
    }

    private async UniTask WaitForSceneLoad(AsyncOperation asyncOp) {
        while (!asyncOp.isDone && asyncOp.progress < 0.9f) {
            await UniTask.Yield();
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

    private async UniTask EnterTransition() {
        await UniTask.Yield();
    }

    private async UniTask ExitTransition() {
        await UniTask.Yield();
    }

    private void MoveScene(Scene scene, Transform newPlace) {
        GameObject newRoot = new GameObject();
        GameObject[] roots = scene.GetRootGameObjects();

        foreach (var root in roots) {
            root.transform.SetParent(newRoot.transform);
        }

        newRoot.transform.SetPositionAndRotation(newPlace.position, newPlace.rotation);
    }
}
