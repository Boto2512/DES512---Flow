using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLaunchPad : MonoBehaviour {
    [SerializeField] float launchForce;
    [SerializeField] string loadScene;
    [SerializeField] float wait;
    [Tooltip("Link to the animator on Player UI Canvas called Fade")]
    [SerializeField] Animator fadeAnimation;

    [SerializeField] Transform[] sections;
    private bool unlocked;
    [SerializeField] bool bypassEnemies;

    private LevelManager levelManager;

    private void Start() {
        levelManager = GameObject.FindAnyObjectByType<LevelManager>();
    }

    private void Update() {
        if (bypassEnemies) { unlocked = true; }
        else {
            foreach (Transform section in sections) {
                if (section.childCount != 0) {
                    unlocked = false;
                    break;
                }
                else {
                    unlocked = true;
                }
            }
        }
    }
    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.CompareTag("Player") && unlocked) {
            other.attachedRigidbody.GetComponent<IMomentumModifiable>().SetMomentum(Vector3.up * launchForce);
            //StartCoroutine(LoadNextLevel());
            levelManager.TrampleSteamUsed.Invoke();
        }
    }

    private IEnumerator LoadNextLevel() {
        fadeAnimation.SetTrigger("startFade");
        yield return new WaitForSeconds(wait);
        AsyncOperation loadAsyncOperation = SceneManager.LoadSceneAsync(loadScene);

    }
}
