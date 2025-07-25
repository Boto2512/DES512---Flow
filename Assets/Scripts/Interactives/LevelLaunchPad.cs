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
    [SerializeField] TextMeshProUGUI completionTimer;
    private float timer;
    private bool isTiming = true;

    [SerializeField] Transform[] sections;
    private bool unlocked;
    [SerializeField] bool bypassEnemies;

    private void Start()
    {
        completionTimer.enabled = false;
    }

    private void Update() {

        if(isTiming) { timer += Time.deltaTime; }

        if(bypassEnemies) { unlocked = true; }
        else { 
            foreach (Transform section in sections) {
                if(section.childCount != 0) {
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
            isTiming = false;
            completionTimer.text = $"Level Complete: \n{(Mathf.Round(timer * 100)/100)}s";

            other.attachedRigidbody.GetComponent<IMomentumModifiable>().SetMomentum(Vector3.up * launchForce);
            StartCoroutine(LoadNextLevel());
        }
    }

    private IEnumerator LoadNextLevel() {
        fadeAnimation.SetTrigger("startFade");
        yield return new WaitForSeconds(wait);
        completionTimer.enabled = true;
        yield return new WaitForSeconds(2.5f);
        AsyncOperation loadAsyncOperation = SceneManager.LoadSceneAsync(loadScene);
    }

    private void LevelComplete()
    {
        LevelTimer.instance.SaveTime();
    }
}
