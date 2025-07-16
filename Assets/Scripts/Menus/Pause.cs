using UnityEngine;
using UnityEngine.SceneManagement;

public class Pause : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject optionsMenu;
    public static bool isPaused;

    private void Start()
    {
        isPaused = false;
        pauseMenu.SetActive(false);
    }
    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape) && !optionsMenu.activeSelf) { PauseGame(!isPaused); }
        if (Input.GetKeyDown(KeyCode.R)) {
            Retry();
        }
    }

    public void ToMainMenu() {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        Time.timeScale = 1;
        SceneManager.LoadScene(0);
    }

    public void Retry()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    public void PauseGame(bool pause)
    {
        if (pause) {
            isPaused = true;

            Time.timeScale = 0;

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            pauseMenu.SetActive(true);
        }
        else {
            isPaused = false;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;

            if (SceneManager.GetActiveScene().name == "MainMenu") {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }

            Time.timeScale = 1; 
            pauseMenu.SetActive(false);
        }
    }
}
