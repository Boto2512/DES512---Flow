using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLaunchPad : MonoBehaviour
{
    [SerializeField] float launchForce;
    [SerializeField] string loadScene;
    [SerializeField] float wait;
    [Tooltip("Link to the animator on Player UI Canvas called Fade")]
    [SerializeField] Animator fadeAnimation;
    [Header("Timer")]
    TextMeshProUGUI completionTimer;
    [SerializeField] GameObject completionCanvas;
    private float timer;
    private bool isTiming = true;

    [SerializeField] Transform[] sections;
    private bool unlocked;
    private int enemyCount;
    TextMeshProUGUI enemyCounter;
    [SerializeField] bool bypassEnemies;
    [Header("Level Timer")]
    [SerializeField] float levelTimer;
    private bool stopTimer;

    private void Start()
    {
        completionCanvas = GameObject.FindWithTag("CompletionCanvas");
        completionTimer = completionCanvas.GetComponentInChildren<TextMeshProUGUI>();
        enemyCounter = gameObject.GetComponentInChildren<TextMeshProUGUI>();
        completionCanvas.SetActive(false);

        foreach (Transform section in sections)
        {
            if (section.childCount != 0)
            {
                enemyCount += section.childCount;
                break;
            }
        }
    }

    private void Update() {
        if (!stopTimer) { levelTimer += Time.deltaTime; }

        if(isTiming) { timer += Time.deltaTime; }

        if(bypassEnemies) { unlocked = true; }
        else {
            int enemies = 0;
            foreach (Transform section in sections) {
                if(section.childCount != 0) {
                    unlocked = false;
                    enemies += section.childCount;
                    break;
                } 
                else {
                    unlocked = true;
                    //enemyCounter.enabled = false;
                }
            }
            enemyCount = enemies;
        }
            enemyCounter.text = enemyCount.ToString();
            if(enemyCount == 0) enemyCounter.enabled = false;

    }
    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.CompareTag("Player") && unlocked) {
            isTiming = false;
            completionTimer.text = $"Level Complete: \n{(Mathf.Round(timer * 100)/100)}s";

            other.attachedRigidbody.GetComponent<IMomentumModifiable>().SetMomentum(Vector3.up * launchForce);
            stopTimer = true;
            StartCoroutine(LoadNextLevel());
        }
    }

    private IEnumerator LoadNextLevel() {
        fadeAnimation.SetTrigger("startFade");
        yield return new WaitForSeconds(wait);
        completionCanvas.SetActive(true);
        yield return new WaitForSeconds(2.5f);
        AsyncOperation loadAsyncOperation = SceneManager.LoadSceneAsync(loadScene);
    }
}
