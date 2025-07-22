using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class Pause : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject optionsMenu;
    public static bool isPaused;

    [SerializeField] private GameObject firstSelectedPause;
    [SerializeField] private GameObject firstSelectedSettings;

    private void Start()
    {
        isPaused = false;
        pauseMenu.SetActive(false);
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) {
            Retry();
        }
    }

    public void OnPause(InputAction.CallbackContext pauseContext)
    {
        if (pauseContext.started)
        {
            PauseGame(!isPaused);
            optionsMenu.SetActive(false);
            EventSystem.current.SetSelectedGameObject(firstSelectedPause);
        }
    }

    public void ToSettings()
    {
        optionsMenu.SetActive(true);
        EventSystem.current.SetSelectedGameObject(firstSelectedSettings);
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
